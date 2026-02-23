package examples_test

import (
	"context"
	"database/sql"
	"testing"

	_ "github.com/lib/pq"
	"github.com/stretchr/testify/require"
	"github.com/testcontainers/testcontainers-go"
	"github.com/testcontainers/testcontainers-go/modules/postgres"
)

// TestPostgresVersionMatrix demonstrates table-driven testing against
// multiple PostgreSQL versions. This pattern is useful for verifying
// compatibility across different database versions in CI pipelines.
func TestPostgresVersionMatrix(t *testing.T) {
	versions := []struct {
		name  string
		image string
	}{
		{"Postgres 14", "postgres:14-alpine"},
		{"Postgres 15", "postgres:15-alpine"},
		{"Postgres 16", "postgres:16-alpine"},
	}

	for _, tc := range versions {
		t.Run(tc.name, func(t *testing.T) {
			t.Parallel() // Run versions in parallel

			ctx := context.Background()

			pgContainer, err := postgres.Run(ctx, tc.image,
				postgres.WithDatabase("testdb"),
				postgres.BasicWaitStrategies(),
			)
			testcontainers.CleanupContainer(t, pgContainer)
			require.NoError(t, err)

			connStr, err := pgContainer.ConnectionString(ctx, "sslmode=disable")
			require.NoError(t, err)

			db, err := sql.Open("postgres", connStr)
			require.NoError(t, err)
			defer db.Close()

			// Verify the version matches expectations
			var version string
			err = db.QueryRow("SHOW server_version").Scan(&version)
			require.NoError(t, err)
			t.Logf("Connected to PostgreSQL version: %s", version)

			// Verify basic operations work across versions
			_, err = db.Exec(`CREATE TABLE compat_test (id SERIAL PRIMARY KEY, data TEXT)`)
			require.NoError(t, err)

			_, err = db.Exec(`INSERT INTO compat_test (data) VALUES ($1)`, "test")
			require.NoError(t, err)

			var data string
			err = db.QueryRow(`SELECT data FROM compat_test WHERE id = 1`).Scan(&data)
			require.NoError(t, err)
			require.Equal(t, "test", data)
		})
	}
}

// TestPostgresWithSnapshotIsolation demonstrates using PostgreSQL snapshots
// to run multiple subtests against the same initial database state.
// Each subtest modifies the database, then the snapshot is restored
// before the next subtest runs — much faster than restarting the container.
func TestPostgresWithSnapshotIsolation(t *testing.T) {
	ctx := context.Background()

	pgContainer, err := postgres.Run(ctx, "postgres:16-alpine",
		postgres.WithDatabase("snapshotdb"),
		postgres.BasicWaitStrategies(),
	)
	testcontainers.CleanupContainer(t, pgContainer)
	require.NoError(t, err)

	connStr, err := pgContainer.ConnectionString(ctx, "sslmode=disable")
	require.NoError(t, err)

	db, err := sql.Open("postgres", connStr)
	require.NoError(t, err)
	defer db.Close()

	// Set up initial state
	_, err = db.Exec(`CREATE TABLE accounts (id SERIAL PRIMARY KEY, name TEXT, balance NUMERIC(10,2))`)
	require.NoError(t, err)
	_, err = db.Exec(`INSERT INTO accounts (name, balance) VALUES ('Alice', 100.00), ('Bob', 50.00)`)
	require.NoError(t, err)

	// Close connection before snapshot (PostgreSQL can't snapshot with active connections)
	db.Close()

	// Take a snapshot of the initial state
	err = pgContainer.Snapshot(ctx, postgres.WithSnapshotName("initial-state"))
	require.NoError(t, err)

	// Reconnect after snapshot
	db, err = sql.Open("postgres", connStr)
	require.NoError(t, err)

	t.Run("Transfer", func(t *testing.T) {
		// Transfer from Alice to Bob
		_, err := db.Exec(`UPDATE accounts SET balance = balance - 25 WHERE name = 'Alice'`)
		require.NoError(t, err)
		_, err = db.Exec(`UPDATE accounts SET balance = balance + 25 WHERE name = 'Bob'`)
		require.NoError(t, err)

		var aliceBalance float64
		err = db.QueryRow(`SELECT balance FROM accounts WHERE name = 'Alice'`).Scan(&aliceBalance)
		require.NoError(t, err)
		require.Equal(t, 75.0, aliceBalance)
	})

	// Close connection before restore (PostgreSQL can't restore with active connections)
	db.Close()

	// Restore to initial state before next subtest
	err = pgContainer.Restore(ctx, postgres.WithSnapshotName("initial-state"))
	require.NoError(t, err)

	// Reconnect after restore
	db, err = sql.Open("postgres", connStr)
	require.NoError(t, err)
	defer db.Close()

	t.Run("DeleteAccount", func(t *testing.T) {
		// Alice should have her original balance again
		var aliceBalance float64
		err := db.QueryRow(`SELECT balance FROM accounts WHERE name = 'Alice'`).Scan(&aliceBalance)
		require.NoError(t, err)
		require.Equal(t, 100.0, aliceBalance, "Snapshot should have restored Alice's original balance")

		_, err = db.Exec(`DELETE FROM accounts WHERE name = 'Bob'`)
		require.NoError(t, err)

		var count int
		err = db.QueryRow(`SELECT COUNT(*) FROM accounts`).Scan(&count)
		require.NoError(t, err)
		require.Equal(t, 1, count)
	})
}
