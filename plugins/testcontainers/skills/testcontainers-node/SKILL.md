---
name: testcontainers-node
description: >
  A comprehensive guide for using Testcontainers for Node.js to write reliable integration tests
  with Docker containers in Node.js/TypeScript projects. Supports pre-configured modules for databases,
  message queues, cloud services, and more. Use this skill when writing Node.js integration tests,
  setting up test databases (PostgreSQL, MySQL, Redis, MongoDB), testing with message queues
  (Kafka, RabbitMQ), or creating container-based test infrastructure. Covers modules, generic
  containers, networking, cleanup, wait strategies, Docker Compose, and common testing patterns.
applies_to: Testcontainers for Node.js
license: MIT
---

# Testcontainers for Node.js Integration Testing

You are an expert Node.js/TypeScript developer specializing in integration testing with Testcontainers. When this skill is active, you should:

- **Always prefer pre-configured modules** over generic containers when a module exists
- **Use proper wait strategies** instead of `setTimeout()` or arbitrary delays -- never suggest delays as a synchronization mechanism
- **Generate complete, runnable test code** including all necessary imports
- **Apply Node.js testing conventions** for the test runner in use (Jest, Vitest, Mocha, Node test runner, Bun, Deno, etc.)
- **Always stop containers in `afterAll`/`afterEach`** to prevent resource leaks
- **Use TypeScript** by default unless the project is JavaScript-only

## Description

This skill helps you write integration tests using Testcontainers for Node.js, a library that provides lightweight, throwaway instances of common databases, message queues, web browsers, or anything that can run in a Docker container.

**Key capabilities:**
- Use many pre-configured modules for common services (databases, message queues, cloud services, etc.)
- Set up and manage Docker containers in Node.js tests
- Configure networking, volumes, and environment variables
- Implement proper cleanup and resource management
- Debug and troubleshoot container issues

## When to Use This Skill

**Trigger keywords:** integration test, testcontainers, docker test, container test, database test, test with postgres, test with redis, test with kafka, real database test, end-to-end test infrastructure, test environment setup, test cleanup, test isolation.

Use this skill when you need to:
- Write integration tests that require real services (databases, message queues, etc.)
- Test against multiple versions or configurations of dependencies
- Create reproducible test environments
- Avoid mocking external dependencies in integration tests
- Set up ephemeral test infrastructure
- Run integration tests in CI/CD pipelines

## Prerequisites

- **Docker or compatible runtime** installed and running (Docker, Podman, Colima, Rancher Desktop)
- **Node.js** current LTS version or later
- **Docker socket** accessible at standard locations
- **Test framework**: Any JavaScript/TypeScript test runner (Jest, Vitest, Mocha, Node.js built-in test runner, Bun, Deno, etc.)

## Decision Guide

Use this decision tree to choose the right approach:

```
Need a container for testing?
|-- Is there a pre-configured module? (check module list below)
|   |-- YES -> Use the module (Section 2)
|   +-- NO  -> Use GenericContainer (Section 3)
|
|-- Need multiple containers to communicate?
|   +-- YES -> Create a custom network (Section 5)
|
|-- Have a docker-compose.yml already?
|   +-- YES -> Use DockerComposeEnvironment (Section 6)
|
|-- Need shared setup across all test files?
|   +-- YES -> Use global setup (Section 10)
|
+-- Running in CI/CD?
    +-- YES -> See CI/CD Integration (Section 11)
```

## Instructions

### 1. Installation & Setup

Install the core package as a dev dependency:

```bash
# npm
npm install testcontainers --save-dev

# yarn
yarn add testcontainers --dev

# pnpm
pnpm add testcontainers --save-dev
```

For pre-configured modules (recommended):

```bash
# Example: PostgreSQL module
npm install @testcontainers/postgresql --save-dev

# Example: Redis module
npm install @testcontainers/redis --save-dev

# Example: Kafka module
npm install @testcontainers/kafka --save-dev
```

**Module package naming convention:** `@testcontainers/<module-name>`

---

### 2. Using Pre-Configured Modules (Recommended Approach)

**Testcontainers for Node.js provides many pre-configured modules** that offer production-ready configurations, sensible defaults, and helper methods. **Always prefer modules over generic containers** when available.

#### Why Use Modules?

- **Sensible defaults**: Pre-configured ports, environment variables, and wait strategies
- **Connection helpers**: Built-in methods like `getConnectionUrl()`, `getConnectionUri()`, `getConnectionString()`
- **Specialized features**: Module-specific functionality (e.g., PostgreSQL snapshots, Kafka SASL/SSL)
- **Automatic credentials**: Secure credential generation and management
- **Battle-tested**: Used in production by many projects

#### Available Modules

Browse the full module catalog at: https://node.testcontainers.org/ (see sidebar for complete list)

Modules cover databases, message queues, search engines, cloud services, and more. The catalog is the authoritative source for available modules.

#### Basic Module Usage Pattern

```typescript
import { RedisContainer, StartedRedisContainer } from "@testcontainers/redis";
import { createClient, RedisClientType } from "redis";

describe("Redis", () => {
  let container: StartedRedisContainer;
  let redisClient: RedisClientType;

  beforeAll(async () => {
    container = await new RedisContainer("redis:8").start();
    redisClient = createClient({ url: container.getConnectionUrl() });
    await redisClient.connect();
  });

  afterAll(async () => {
    await redisClient.disconnect();
    await container.stop();
  });

  it("stores and retrieves a value", async () => {
    await redisClient.set("key", "val");
    expect(await redisClient.get("key")).toBe("val");
  });
});
```

#### Finding the Right Module

1. **Browse the module catalog**: https://node.testcontainers.org/ (sidebar lists all modules with documentation)
2. **Check the GitHub repository**: `packages/modules/` in [testcontainers-node](https://github.com/testcontainers/testcontainers-node)
3. **Module package pattern**: `@testcontainers/<module-name>`

Each module page in the docs includes API details, helper methods, and usage examples specific to that service.

---

### 3. Using Generic Containers (Fallback)

When no pre-configured module exists, use `GenericContainer`.

**IMPORTANT: Always add a wait strategy when using generic containers** to ensure the container is ready before tests run. This is critical for reliability, especially in CI environments.

```typescript
import { GenericContainer, StartedTestContainer, Wait } from "testcontainers";

describe("Custom Service", () => {
  let container: StartedTestContainer;

  beforeAll(async () => {
    container = await new GenericContainer("custom-image:latest")
      .withExposedPorts(8080)
      .withEnvironment({ APP_ENV: "test" })
      .withWaitStrategy(Wait.forListeningPorts())
      .start();
  });

  afterAll(async () => {
    await container.stop();
  });

  it("responds to requests", async () => {
    const host = container.getHost();
    const port = container.getMappedPort(8080);
    const url = `http://${host}:${port}`;
    // Make requests to the service...
  });
});
```

**Core GenericContainer API (fluent builder pattern):**

```typescript
const container = await new GenericContainer("image:tag")
  // Ports
  .withExposedPorts(80, 443)
  .withExposedPorts({ container: 80, protocol: "udp" })
  .withExposedPorts("80/udp")

  // Environment & Commands
  .withEnvironment({ KEY: "value", ANOTHER: "val" })
  .withCommand(["sleep", "infinity"])
  .withEntrypoint(["cat"])
  .withWorkingDir("/opt")

  // User & Security
  .withUser("bob")
  .withPrivilegedMode()
  .withSecurityOpt("no-new-privileges")
  .withAddedCapabilities("NET_ADMIN", "IPC_LOCK")
  .withDroppedCapabilities("NET_ADMIN")

  // Labels & Name
  .withLabels({ label: "value" })
  .withName("custom-container-name")  // Not recommended
  .withHostname("my-hostname")        // Not recommended

  // Files & Mounts
  .withCopyFilesToContainer([{
    source: "/local/file.txt",
    target: "/remote/file.txt",
    mode: parseInt("0644", 8),
  }])
  .withCopyDirectoriesToContainer([{
    source: "/local/dir",
    target: "/remote/dir",
  }])
  .withCopyContentToContainer([{
    content: "hello world",
    target: "/remote/file.txt",
  }])
  .withBindMounts([{
    source: "/host/path",
    target: "/container/path",
    mode: "ro",
  }])
  .withTmpFs({ "/temp": "rw,noexec,nosuid,size=65536k" })

  // Resources
  .withResourcesQuota({ memory: 0.5, cpu: 1 })  // memory in GB
  .withSharedMemorySize(512 * 1024 * 1024)
  .withUlimits({ memlock: { hard: -1, soft: -1 } })
  .withIpcMode("host")

  // Networking
  .withNetwork(network)
  .withNetworkMode("bridge")
  .withNetworkAliases("my-alias")
  .withExtraHosts([{ host: "foo", ipAddress: "10.11.12.13" }])

  // Logging
  .withLogConsumer(stream => {
    stream.on("data", line => console.log(line));
    stream.on("err", line => console.error(line));
    stream.on("end", () => console.log("Stream closed"));
  })
  .withDefaultLogDriver()

  // Wait Strategy
  .withWaitStrategy(Wait.forListeningPorts())
  .withStartupTimeout(60_000)

  // Image
  .withPullPolicy(PullPolicy.alwaysPull())
  .withPlatform("linux/arm64")

  // Lifecycle
  .withReuse()
  .withAutoRemove(false)

  .start();
```

**Retrieving container information after start:**

```typescript
const host = container.getHost();
const port = container.getMappedPort(80);
const firstPort = container.getFirstMappedPort();
const udpPort = container.getMappedPort(80, "udp");
```

---

### 4. Container Lifecycle

#### Starting

```typescript
const container = await new GenericContainer("alpine").start();
```

#### Stopping

```typescript
await container.stop();
await container.stop({ timeout: 10_000 });     // custom timeout (ms)
await container.stop({ remove: false });        // keep container after stop
await container.stop({ removeVolumes: false }); // preserve volumes
```

#### Restarting

```typescript
await container.restart();
```

#### Executing Commands

```typescript
const { output, stdout, stderr, exitCode } = await container.exec(
  ["echo", "hello", "world"],
  {
    workingDir: "/app/src/",
    user: "1000:1000",
    env: { VAR1: "enabled", VAR2: "/app/debug.log" },
  }
);
```

#### Reading Logs After Start

```typescript
(await container.logs())
  .on("data", line => console.log(line))
  .on("err", line => console.error(line))
  .on("end", () => console.log("Stream closed"));

// With timestamp filtering
const tenSecondsAgoMs = new Date().getTime() - 10 * 1000;
const since = tenSecondsAgoMs / 1000;
(await container.logs({ since }))
  .on("data", line => console.log(line));
```

#### Copying Files After Start

```typescript
// Copy to container
container.copyFilesToContainer([{ source: "/local/path", target: "/remote/path" }]);
container.copyContentToContainer([{ content: "data", target: "/remote/file" }]);

// Copy from container
const tarArchiveStream = await container.copyArchiveFromContainer("/var/log");

// Copy from stopped container
const stoppedContainer = await container.stop({ remove: false });
const archive = await stoppedContainer.copyArchiveFromContainer("/var/log/syslog");
```

#### Committing a Container to an Image

```typescript
const newImageId = await container.commit({
  repo: "my-repo",
  tag: "my-tag",
  deleteOnExit: false,
});
const newContainer = await new GenericContainer(newImageId).start();
```

#### Container Reuse

Reuse containers across test runs for faster iteration:

```typescript
const container = await new GenericContainer("alpine")
  .withCommand(["sleep", "infinity"])
  .withReuse()
  .start();
```

Control reuse globally via the `TESTCONTAINERS_REUSE_ENABLE` environment variable (enabled by default).

---

### 5. Container Networking

#### Port Mapping

Testcontainers automatically maps container ports to random available host ports:

```typescript
const container = await new GenericContainer("nginx")
  .withExposedPorts(80)
  .start();

const host = container.getHost();
const port = container.getMappedPort(80);
// Access at http://${host}:${port}
```

**Supported port formats:**

```typescript
.withExposedPorts(80, 443)                            // TCP (default)
.withExposedPorts({ container: 80, protocol: "udp" }) // UDP
.withExposedPorts("80/udp")                           // UDP string format
.withExposedPorts({ container: 80, host: 8080 })      // Fixed host port (not recommended)
```

#### Creating Networks

```typescript
import { Network, GenericContainer } from "testcontainers";

const network = await new Network().start();

const container1 = await new GenericContainer("service-a")
  .withNetwork(network)
  .withNetworkAliases("service-a")
  .start();

const container2 = await new GenericContainer("service-b")
  .withNetwork(network)
  .withNetworkAliases("service-b")
  .start();

// container2 can reach container1 via "service-a:port"

// Get IP address on the network
const ip = container1.getIpAddress(network.getName());

// Cleanup
await container1.stop();
await container2.stop();
await network.stop();
```

#### Network Aliases (Preferred for Container-to-Container Communication)

```typescript
const network = await new Network().start();

const db = await new GenericContainer("postgres:16")
  .withNetwork(network)
  .withNetworkAliases("database")
  .withEnvironment({ POSTGRES_PASSWORD: "test" })
  .start();

const app = await new GenericContainer("myapp:latest")
  .withNetwork(network)
  .withEnvironment({
    DB_HOST: "database",  // Use network alias, not localhost
    DB_PORT: "5432",      // Use internal port, not mapped port
  })
  .start();
```

#### Network Modes

```typescript
.withNetworkMode("bridge")
.withNetworkMode("host")  // Linux only
```

#### Exposing Host Ports to Containers

```typescript
import { TestContainers } from "testcontainers";

// Make host port 3000 accessible from within containers
TestContainers.exposeHostPorts(3000);

// Inside the container, access via:
// host.testcontainers.internal:3000
```

This launches an SSHd container for remote port forwarding.

#### Extra Hosts

```typescript
.withExtraHosts([
  { host: "foo", ipAddress: "10.11.12.13" },
  { host: "bar", ipAddress: "11.12.13.14" },
])
```

---

### 6. Docker Compose Support

Use `DockerComposeEnvironment` when you have existing `docker-compose.yml` files:

```typescript
import { DockerComposeEnvironment, StartedDockerComposeEnvironment, Wait } from "testcontainers";

describe("Multi-service", () => {
  let environment: StartedDockerComposeEnvironment;

  beforeAll(async () => {
    environment = await new DockerComposeEnvironment("/path/to/project", "docker-compose.yml")
      .withWaitStrategy("db-1", Wait.forLogMessage("Ready to accept connections"))
      .withWaitStrategy("app-1", Wait.forHealthCheck())
      .up();
  });

  afterAll(async () => {
    await environment.down();
  });

  it("can access services", async () => {
    const dbContainer = environment.getContainer("db-1");
    const dbPort = dbContainer.getMappedPort(5432);
  });
});
```

**DockerComposeEnvironment methods:**

```typescript
new DockerComposeEnvironment(composeFilePath, composeFile)
  // Starting
  .up()                                        // start all services
  .up(["redis", "postgres"])                   // start specific services

  // Wait strategies
  .withWaitStrategy("service-1", strategy)     // per-service wait
  .withDefaultWaitStrategy(strategy)           // default for all services

  // Configuration
  .withBuild()                                 // rebuild images
  .withPullPolicy(policy)                      // image pull behavior
  .withEnvironmentFile("/path/.env")           // load env file
  .withEnvironment({ VAR: "value" })           // set env vars
  .withProfiles("profile1", "profile2")        // activate compose profiles
  .withNoRecreate()                            // prevent container recreation
  .withProjectName("my-project")               // custom project name

  // Stopping
  .down()                                      // stop (no wait)
  .down({ timeout: 10_000 })                   // stop with timeout
  .down({ removeVolumes: false })              // preserve volumes
  .stop()                                      // halt without removing
```

**Multiple compose files (for overrides):**

```typescript
const environment = await new DockerComposeEnvironment(
  "/path/to/project",
  ["docker-compose.yml", "docker-compose.test.yml"]
).up();
```

---

### 7. Building Custom Images

Build images from a Dockerfile within your tests:

```typescript
import { GenericContainer } from "testcontainers";

// Basic build
const container = await GenericContainer
  .fromDockerfile("/path/to/build-context")
  .build();
const started = await container.start();

// Named image (persisted between runs)
const container = await GenericContainer
  .fromDockerfile("/path/to/build-context")
  .build("my-custom-image", { deleteOnExit: false });

// Build with options
const container = await GenericContainer
  .fromDockerfile("/path/to/build-context")
  .withBuildArgs({ ARG: "VALUE" })
  .withTarget("my-stage")           // multi-stage build target
  .withCache(false)                  // disable cache
  .withPlatform("linux/amd64")      // target platform
  .withBuildkit()                    // use BuildKit
  .withPullPolicy(PullPolicy.alwaysPull())
  .build();

// Custom Dockerfile name
const container = await GenericContainer
  .fromDockerfile("/path/to/build-context", "my-dockerfile")
  .build();
```

---

### 8. Wait Strategies

Wait strategies ensure containers are ready before tests run. **Always use an explicit wait strategy for generic containers.**

#### Listening Ports (Default)

Waits for mapped network ports to be bound:

```typescript
import { GenericContainer, Wait } from "testcontainers";

const container = await new GenericContainer("alpine")
  .withExposedPorts(6379)
  .withWaitStrategy(Wait.forListeningPorts())
  .start();
```

#### Log Output

Waits for specific log messages:

```typescript
// String match
.withWaitStrategy(Wait.forLogMessage("Ready to accept connections"))

// Regex match
.withWaitStrategy(Wait.forLogMessage(/Listening on port \d+/))

// Multiple occurrences
.withWaitStrategy(Wait.forLogMessage("Listening on port 8080", 2))
```

#### Health Check

Waits for container health check to pass:

```typescript
// Use image's built-in health check
.withWaitStrategy(Wait.forHealthCheck())

// Custom health check
.withHealthCheck({
  test: ["CMD-SHELL", "curl -f http://localhost || exit 1"],
  interval: 1000,
  timeout: 3000,
  retries: 5,
  startPeriod: 1000,
})
.withWaitStrategy(Wait.forHealthCheck())

// For distroless images (no shell):
.withHealthCheck({
  test: ["CMD", "executable", "arg1", "arg2"],
  // ...
})
```

#### HTTP

Waits for HTTP endpoint conditions:

```typescript
// Basic (expects 200)
.withWaitStrategy(Wait.forHttp("/health", 8080))

// Abort if container exits
.withWaitStrategy(Wait.forHttp("/health", 8080)
  .abortOnContainerExit(true))

// Custom status code
.withWaitStrategy(Wait.forHttp("/health", 8080)
  .forStatusCode(201))

// Status code matcher
.withWaitStrategy(Wait.forHttp("/health", 8080)
  .forStatusCodeMatching(statusCode => `${statusCode}`.startsWith("2")))

// Response body validation
.withWaitStrategy(Wait.forHttp("/health", 8080)
  .forResponsePredicate(response => response === "OK"))

// Custom request options
.withWaitStrategy(Wait.forHttp("/health", 8080)
  .withMethod("POST")
  .withHeaders({ X_CUSTOM_VALUE: "custom" })
  .withBasicCredentials("username", "password")
  .withReadTimeout(10_000))

// TLS
.withWaitStrategy(Wait.forHttp("/health", 8443)
  .usingTls()
  .insecureTls())  // skip certificate verification
```

#### Shell Command

Waits for a command to succeed (exit code 0):

```typescript
.withWaitStrategy(Wait.forSuccessfulCommand("stat /tmp/app.lock"))
```

#### One Shot Startup

For containers that run a task and exit (exit code 0 = success):

```typescript
.withWaitStrategy(Wait.forOneShotStartup())
```

#### Composite (Multiple Strategies)

Chain multiple strategies:

```typescript
.withWaitStrategy(Wait.forAll([
  Wait.forListeningPorts(),
  Wait.forLogMessage("Ready to accept connections"),
]))
```

**Composite timeout behavior:**
- Individual strategies retain their own timeouts if set
- Unset timeouts inherit from composite's `.withStartupTimeout()`
- Use `.withDeadline(ms)` to enforce an overall deadline

```typescript
const w1 = Wait.forListeningPorts().withStartupTimeout(1000);
const w2 = Wait.forLogMessage("READY");
const composite = Wait.forAll([w1, w2]).withStartupTimeout(2000);
```

#### Custom Wait Strategy

Extend `StartupCheckStrategy` for specialized requirements:

```typescript
import { GenericContainer, StartupCheckStrategy, StartupStatus } from "testcontainers";

class ReadyAfterDelayWaitStrategy extends StartupCheckStrategy {
  checkStartupState(dockerClient, containerId) {
    return new Promise((resolve) =>
      setTimeout(() => resolve("SUCCESS"), 3000)
    );
  }
}

const container = await new GenericContainer("alpine")
  .withWaitStrategy(new ReadyAfterDelayWaitStrategy())
  .start();
```

#### Startup Timeout

Default startup timeout is 60 seconds. Override per wait strategy:

```typescript
.withWaitStrategy(
  Wait.forLogMessage("Ready").withStartupTimeout(120_000)  // 2 minutes
)
```

Or globally on the container:

```typescript
.withStartupTimeout(120_000)
```

---

### 9. Writing Integration Tests

#### Test Structure Best Practices

```typescript
import { GenericContainer, StartedTestContainer } from "testcontainers";

describe("MyService", () => {
  let container: StartedTestContainer;

  beforeAll(async () => {
    // 1. Start container
    container = await new GenericContainer("redis:8")
      .withExposedPorts(6379)
      .start();

    // 2. Connect to service
    // ... initialize client using container.getHost() and container.getMappedPort()
  }, 60_000);

  afterAll(async () => {
    // 3. Disconnect client
    // ... close client connections

    // 4. Stop container (ALWAYS do this)
    await container.stop();
  });

  it("performs an operation", async () => {
    // 5. Test your application logic
  });
});
```

**Critical: Always stop containers in afterAll/afterEach to prevent resource leaks.**

#### Using Modules vs. Generic Containers

```typescript
// WITH MODULE (preferred) - simpler, more features
import { RedisContainer } from "@testcontainers/redis";

const container = await new RedisContainer("redis:8").start();
const url = container.getConnectionUrl(); // Built-in connection helper

// WITHOUT MODULE (generic) - more configuration needed
import { GenericContainer } from "testcontainers";

const container = await new GenericContainer("redis:8")
  .withExposedPorts(6379)
  .start();
const url = `redis://${container.getHost()}:${container.getMappedPort(6379)}`;
```

---

### 10. Test Runner Patterns

#### Jest

```typescript
// jest.config.ts - increase timeout for container startup
export default {
  testTimeout: 60_000,
};
```

```typescript
import { PostgreSqlContainer, StartedPostgreSqlContainer } from "@testcontainers/postgresql";

describe("Database tests", () => {
  let container: StartedPostgreSqlContainer;

  beforeAll(async () => {
    container = await new PostgreSqlContainer("postgres:16-alpine").start();
  }, 60_000); // timeout for beforeAll

  afterAll(async () => {
    await container.stop();
  });

  it("works", async () => {
    // test logic
  });
});
```

#### Vitest

```typescript
// vitest.config.ts
import { defineConfig } from "vitest/config";

export default defineConfig({
  test: {
    testTimeout: 60_000,
    hookTimeout: 60_000,
  },
});
```

**Vitest Global Setup (shared containers across all test files):**

```typescript
// setup.ts
import { RedisContainer } from "@testcontainers/redis";

let redisContainer;

export async function setup(project) {
  redisContainer = await new RedisContainer("redis:8").start();
  project.provide("redisUrl", redisContainer.getConnectionUrl());
}

export async function teardown() {
  await redisContainer?.stop();
}
```

```typescript
// vitest.config.ts
import { defineConfig } from "vitest/config";

export default defineConfig({
  test: {
    globalSetup: "./setup.ts",
  },
});
```

```typescript
// my-test.test.ts
import { inject, beforeAll, afterAll, test, expect } from "vitest";
import { createClient } from "redis";

const redisClient = createClient({ url: inject("redisUrl") });

beforeAll(async () => {
  await redisClient.connect();
});

afterAll(async () => {
  await redisClient.disconnect();
});

test("stores and reads a value", async () => {
  await redisClient.set("key", "test-value");
  const result = await redisClient.get("key");
  expect(result).toBe("test-value");
});
```

**Note:** `globalSetup` runs in a different global scope than test files. To share data with tests, provide serializable values in `setup` and read them with `inject`.

#### Node.js Built-in Test Runner

```typescript
import { describe, it, before, after } from "node:test";
import assert from "node:assert";
import { GenericContainer, StartedTestContainer } from "testcontainers";

describe("MyService", () => {
  let container: StartedTestContainer;

  before(async () => {
    container = await new GenericContainer("redis:8")
      .withExposedPorts(6379)
      .start();
  }, { timeout: 60_000 });

  after(async () => {
    await container.stop();
  });

  it("works", async () => {
    const port = container.getMappedPort(6379);
    assert.ok(port > 0);
  });
});
```

---

### 11. Custom Containers (Extending GenericContainer)

Create reusable container abstractions:

```typescript
import {
  GenericContainer,
  AbstractStartedContainer,
  StartedTestContainer,
  InspectResult,
} from "testcontainers";

class CustomContainer extends GenericContainer {
  constructor() {
    super("my-image:latest");
    this.withExposedPorts(8080);
    this.withEnvironment({ MODE: "test" });
  }

  public async start(): Promise<StartedCustomContainer> {
    return new StartedCustomContainer(await super.start());
  }

  // Lifecycle callbacks (optional)
  protected async beforeContainerCreated(): Promise<void> {}
  protected async containerCreated(containerId: string): Promise<void> {}
  protected async containerStarting(
    inspectResult: InspectResult,
    reused: boolean
  ): Promise<void> {}
  protected async containerStarted(
    container: StartedTestContainer,
    inspectResult: InspectResult,
    reused: boolean
  ): Promise<void> {}
}

class StartedCustomContainer extends AbstractStartedContainer {
  constructor(startedTestContainer: StartedTestContainer) {
    super(startedTestContainer);
  }

  public getApiUrl(): string {
    return `http://${this.getHost()}:${this.getMappedPort(8080)}`;
  }

  // Lifecycle callbacks (optional)
  protected async containerStopping(): Promise<void> {}
  protected async containerStopped(): Promise<void> {}
}
```

---

### 12. Configuration

For the full list of environment variables and configuration options, see the official docs: https://node.testcontainers.org/configuration/

#### Key Configuration

**Logging** uses the `debug` npm package:

```bash
# Enable all testcontainers logs
DEBUG=testcontainers* npm test

# Enable specific log namespaces
DEBUG=testcontainers,testcontainers:exec npm test
```

**Image name substitution** for private registries:

```bash
export TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX=registry.mycompany.com/mirror/
```

**Common environment variables:**
- `DOCKER_HOST` - Docker daemon URL (e.g., `tcp://docker:2375`)
- `TESTCONTAINERS_RYUK_DISABLED=true` - Disable Ryuk (resource reaper)
- `TESTCONTAINERS_REUSE_ENABLE=true` - Enable container reuse

---

### 13. Supported Container Runtimes

#### Docker
Works out of the box with no additional configuration.

#### Podman

**macOS:**
```bash
export DOCKER_HOST=unix://$(podman machine inspect --format '{{.ConnectionInfo.PodmanSocket.Path}}')
export TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE=/var/run/docker.sock
```

**Linux:**
```bash
systemctl --user enable --now podman.socket
export DOCKER_HOST=unix://${XDG_RUNTIME_DIR}/podman/podman.sock
```

**Known issues on macOS:** Ryuk fails in rootless mode. Workarounds:
- Disable Ryuk: `TESTCONTAINERS_RYUK_DISABLED=true`
- Or use rootful mode with: `TESTCONTAINERS_RYUK_PRIVILEGED=true`

#### Colima

```bash
export DOCKER_HOST=unix://${HOME}/.colima/default/docker.sock
export TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE=/var/run/docker.sock
```

**Known issues:**
- No IPv6 support: use `NODE_OPTIONS=--dns-result-order=ipv4first`
- Port forwarding delays: use composite wait strategies combining port listening with other checks

#### Rancher Desktop

```bash
export DOCKER_HOST=unix://${HOME}/.rd/docker.sock
export TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE=/var/run/docker.sock
```

Same known issues as Colima regarding IPv6 and port forwarding.

---

### 14. SocatContainer (TCP Proxy)

Use SocatContainer to proxy TCP traffic between containers:

```typescript
import { SocatContainer } from "testcontainers";

const socat = await new SocatContainer()
  .withNetwork(network)
  .withTarget(8081, "helloworld", 8080)
  .start();

const socatUrl = `http://${socat.getHost()}:${socat.getMappedPort(8081)}`;
```

---

### 15. Advanced: Container Runtime Client

For low-level operations, use the Container Runtime Client:

```typescript
import { getContainerRuntimeClient, ImageName } from "testcontainers";

const client = await getContainerRuntimeClient();

// Pull an image
await client.image.pull(ImageName.fromString("alpine:latest"));

// Create and start containers directly
// Create networks
// Start docker-compose environments
```

---

### 16. Troubleshooting

#### Container Startup Timeout

```typescript
// Increase timeout
.withStartupTimeout(120_000)  // 2 minutes

// Add log consumer to debug
.withLogConsumer(stream => {
  stream.on("data", line => console.log("[container]", line));
})
```

#### Port Already in Use

- Testcontainers auto-assigns random ports -- avoid fixed host port bindings
- Check for leaked containers: `docker ps -a`
- Ensure Ryuk is running to clean up orphaned containers

#### Image Pull Failures

```bash
# Pull manually to verify
docker pull postgres:16

# For private registries
docker login registry.example.com
# Testcontainers uses credentials from ~/.docker/config.json
# Or set DOCKER_AUTH_CONFIG env var
```

#### Resource Leaks

- Always call `container.stop()` in `afterAll`/`afterEach`
- Ryuk (resource reaper) automatically cleans up containers if tests crash
- Check Ryuk is not disabled: ensure `TESTCONTAINERS_RYUK_DISABLED` is not set to `true`

#### Debug Logging

```bash
DEBUG=testcontainers* npm test
```

---

### 17. Common Anti-Patterns

**Anti-pattern: Using `setTimeout` or delays instead of wait strategies**
```typescript
// BAD - flaky and slow
const container = await new GenericContainer("postgres:16")
  .withExposedPorts(5432)
  .start();
await new Promise(resolve => setTimeout(resolve, 5000)); // DO NOT DO THIS

// GOOD - reliable and fast
const container = await new GenericContainer("postgres:16")
  .withExposedPorts(5432)
  .withWaitStrategy(Wait.forLogMessage("ready to accept connections"))
  .start();
```

**Anti-pattern: Fixed host port bindings**
```typescript
// BAD - causes port conflicts in CI
.withExposedPorts({ container: 5432, host: 5432 })

// GOOD - random port, no conflicts
.withExposedPorts(5432)
const port = container.getMappedPort(5432);
```

**Anti-pattern: Not stopping containers**
```typescript
// BAD - resource leak
beforeAll(async () => {
  container = await new GenericContainer("redis").withExposedPorts(6379).start();
});
// Missing afterAll!

// GOOD
afterAll(async () => {
  await container.stop();
});
```

**Anti-pattern: Using generic containers when a module exists**
```typescript
// BAD - reinventing the wheel
const container = await new GenericContainer("postgres:16")
  .withExposedPorts(5432)
  .withEnvironment({
    POSTGRES_DB: "test",
    POSTGRES_USER: "test",
    POSTGRES_PASSWORD: "test",
  })
  .withWaitStrategy(Wait.forLogMessage("ready to accept connections"))
  .start();

// GOOD - module handles all of this
const container = await new PostgreSqlContainer("postgres:16-alpine")
  .withDatabase("test")
  .withUsername("test")
  .start();
```

**Anti-pattern: Hardcoding container hostnames**
```typescript
// BAD - may not work everywhere
const url = `http://localhost:${container.getMappedPort(8080)}`;

// GOOD - works with all container runtimes
const url = `http://${container.getHost()}:${container.getMappedPort(8080)}`;
```

---

### 18. Module Reference

All modules follow the `@testcontainers/<name>` package naming convention. Install with:

```bash
npm install @testcontainers/<module-name> --save-dev
```

For the complete and up-to-date list of available modules, see: https://node.testcontainers.org/
