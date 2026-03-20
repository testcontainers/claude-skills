---
name: testcontainers-node
description: >
  Guide for using Testcontainers for Node.js to write reliable integration tests
  with Docker containers in Node.js/TypeScript projects. Use this skill when writing
  Node.js integration tests, setting up test databases, testing with message queues,
  or creating container-based test infrastructure.
applies_to: Testcontainers for Node.js
license: MIT
---

# Testcontainers for Node.js Integration Testing

You are an expert Node.js/TypeScript developer specializing in integration testing with Testcontainers. When this skill is active, you should:

- **Always prefer pre-configured modules** over generic containers when a module exists
- **Use proper wait strategies** instead of `setTimeout()` or arbitrary delays -- never suggest delays as a synchronization mechanism
- **Generate complete, runnable test code** including all necessary imports
- **Apply testing conventions** for the test runner in use (Jest, Vitest, Mocha, Node test runner, Bun, Deno, etc.)
- **Always stop containers in `afterAll`/`afterEach`** to prevent resource leaks
- **Use TypeScript** by default unless the project is JavaScript-only
- **Always use `container.getHost()`** instead of hardcoding `localhost` -- this ensures compatibility with all container runtimes
- **Infer the package manager** from the project (check for `yarn.lock`, `pnpm-lock.yaml`, `bun.lockb`, or `package-lock.json`) and use the appropriate install command
- **Never use `:latest` image tags** in examples -- always pin to a specific version

## Official Documentation

For API details, code examples, and module-specific usage, refer to the official docs:

- **Docs site**: https://node.testcontainers.org/
- **Module catalog**: https://node.testcontainers.org/ (sidebar lists all modules)
- **Configuration**: https://node.testcontainers.org/configuration/
- **Source code**: https://github.com/testcontainers/testcontainers-node

## Prerequisites

- **Docker or compatible runtime** installed and running (Docker, Podman, Colima, Rancher Desktop)
- **Node.js** current LTS version or later
- **Docker socket** accessible at standard locations

## Decision Guide

```
Need a container for testing?
|-- Is there a pre-configured module? (check module catalog)
|   |-- YES -> Use the module: @testcontainers/<module-name>
|   +-- NO  -> Use GenericContainer with an explicit wait strategy
|
|-- Need multiple containers to communicate?
|   +-- YES -> Create a Network, use .withNetworkAliases() for DNS
|
|-- Have a docker-compose.yml already?
|   +-- YES -> Use DockerComposeEnvironment
|
|-- Need shared setup across all test files?
|   +-- YES -> Use your test runner's global setup
```

## Installation

```bash
# Use the package manager for your project (npm, yarn, pnpm, bun, etc.)
# Core package
npm install -D testcontainers        # npm
yarn add -D testcontainers           # yarn
pnpm add -D testcontainers           # pnpm
bun add -d testcontainers            # bun

# Modules follow the pattern @testcontainers/<module-name>
npm install -D @testcontainers/postgresql
```

## Key Conventions

### 1. Always prefer modules over GenericContainer

Modules provide sensible defaults, connection helpers, and built-in wait strategies. Only use `GenericContainer` when no module exists.

```typescript
// PREFER: module
import { PostgreSqlContainer } from "@testcontainers/postgresql";
const container = await new PostgreSqlContainer().start();
const uri = container.getConnectionUri();

// FALLBACK: generic (always add a wait strategy!)
import { GenericContainer, Wait } from "testcontainers";
const container = await new GenericContainer("custom-image:1.2.3")
  .withExposedPorts(8080)
  .withWaitStrategy(Wait.forListeningPorts())
  .start();
const url = `http://${container.getHost()}:${container.getMappedPort(8080)}`;
```

### 2. Always clean up containers

```typescript
afterAll(async () => {
  await container.stop();
});
```

Never omit cleanup -- leaked containers waste resources and cause port conflicts.

### 3. Always set timeouts for container startup hooks

Container startup can take longer than default test timeouts. Configure this per your test runner:

- **Jest**: `beforeAll(async () => { ... }, 60_000)` and/or `testTimeout` in config
- **Vitest**: `hookTimeout` and `testTimeout` in `vitest.config.ts`
- **Node test runner**: `before(async () => { ... }, { timeout: 60_000 })`

### 4. Always use wait strategies for generic containers

Never rely on timing. Use `Wait.forListeningPorts()`, `Wait.forLogMessage(...)`, `Wait.forHealthCheck()`, `Wait.forHttp(...)`, or `Wait.forAll([...])` for composites.

### 5. Use getHost() not localhost

```typescript
// BAD
const url = `http://localhost:${container.getMappedPort(8080)}`;

// GOOD
const url = `http://${container.getHost()}:${container.getMappedPort(8080)}`;
```

This ensures compatibility with Docker, Podman, Colima, and Rancher Desktop.

### 6. Use random ports, not fixed host bindings

```typescript
// BAD - causes conflicts in CI
.withExposedPorts({ container: 5432, host: 5432 })

// GOOD
.withExposedPorts(5432)
const port = container.getMappedPort(5432);
```

### 7. Container-to-container communication uses network aliases and internal ports

When containers need to talk to each other, use a shared `Network` with `.withNetworkAliases()`. Inside the network, use the alias as hostname and the **internal** container port (not the mapped host port).

### 8. Debug with logging

Enable debug logs with `DEBUG=testcontainers* npm test`. Namespaces: `testcontainers`, `testcontainers:containers`, `testcontainers:compose`, `testcontainers:build`, `testcontainers:pull`, `testcontainers:exec`.

## Common Anti-Patterns to Avoid

| Anti-Pattern | Why It's Bad | What to Do Instead |
|---|---|---|
| `setTimeout`/`await sleep()` after start | Flaky, slow, race conditions | Use wait strategies |
| Fixed host port bindings | Port conflicts in CI | Use random ports + `getMappedPort()` |
| Missing `container.stop()` | Resource leaks | Always stop in `afterAll`/`afterEach` |
| `GenericContainer` when module exists | Reinventing defaults, missing helpers | Check module catalog first |
| Hardcoding `localhost` | Breaks with Podman/Colima/Rancher | Use `container.getHost()` |
| No wait strategy on generic containers | Container may not be ready | Always add `.withWaitStrategy(...)` |
