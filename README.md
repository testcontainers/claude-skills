# Testcontainers Skills for Claude

Skills are folders of instructions, scripts, and resources that Claude loads dynamically to improve performance on specialized tasks. This repository contains Testcontainers-related skills that teach Claude how to work with container-based testing and infrastructure.

For more information about skills, check out:
- [What are skills?](https://support.claude.com/en/articles/12512176-what-are-skills)
- [Using skills in Claude](https://support.claude.com/en/articles/12512180-using-skills-in-claude)
- [How to create custom skills](https://support.claude.com/en/articles/12512198-creating-custom-skills)
- [Equipping agents for the real world with Agent Skills](https://anthropic.com/engineering/equipping-agents-for-the-real-world-with-agent-skills)
- [Main skills repository](https://github.com/anthropics/skills) - Upstream repository with example skills

## About This Repository

This repository contains Testcontainers-related skills for Claude. These skills help Claude work more effectively with container-based testing infrastructure, particularly focusing on integration testing patterns using Docker containers.

Each skill is self-contained in its own directory with a `SKILL.md` file containing the instructions and metadata that Claude uses.

The skills in this repository are open source under the MIT License.

## Disclaimer

**These skills are provided for demonstration and educational purposes only.** While some of these capabilities may be available in Claude, the implementations and behaviors you receive from Claude may differ from what is shown in these examples. These examples are meant to illustrate patterns and possibilities. Always test skills thoroughly in your own environment before relying on them for critical tasks.

## Available Skills

### testcontainers-go
A comprehensive guide for using Testcontainers for Go to write reliable integration tests with Docker containers in Go projects. This skill provides:

- Support for 62+ pre-configured modules for databases, message queues, cloud services, and more
- Best practices for setting up and managing Docker containers in Go tests
- Configuration guidance for networking, volumes, and environment variables
- Proper cleanup and resource management patterns
- Debugging and troubleshooting techniques

**Key capabilities:**
- Use pre-configured modules (PostgreSQL, Redis, Kafka, MySQL, MongoDB, and more)
- Write integration tests with real services instead of mocks
- Test against multiple versions or configurations of dependencies
- Create reproducible test environments
- Set up ephemeral test infrastructure

See the [testcontainers-go skill documentation](./testcontainers-go/SKILL.md) for detailed usage instructions and examples.

## Try in Claude Code, Claude.ai, and the API

### Claude Code
You can register this repository as a Claude Code Plugin marketplace by running the following command in Claude Code:
```
/plugin marketplace add testcontainers/claude-skills
```

Then, to install the testcontainers-go skill:
1. Select `Browse and install plugins`
2. Select `testcontainers-claude-skills`
3. Select `testcontainers-go`
4. Select `Install now`

Alternatively, directly install the plugin via:
```
/plugin install testcontainers-go@testcontainers-claude-skills
```

After installing the plugin, you can use the skill by just mentioning it. For instance: "Use the testcontainers-go skill to help me write an integration test for PostgreSQL"

### Claude.ai

To use any skill from this repository or upload custom skills, follow the instructions in [Using skills in Claude](https://support.claude.com/en/articles/12512180-using-skills-in-claude#h_a4222fa77b).

### Claude API

You can upload custom skills via the Claude API. See the [Skills API Quickstart](https://docs.claude.com/en/api/skills-guide#creating-a-skill) for more.

## Creating a Basic Skill

Skills are simple to create - just a folder with a `SKILL.md` file containing YAML frontmatter and instructions. Here's a basic template:

```markdown
---
name: my-skill-name
description: A clear description of what this skill does and when to use it
license: MIT
---

# My Skill Name

[Add your instructions here that Claude will follow when this skill is active]

## Description

Detailed explanation of the skill's capabilities.

## When to Use This Skill

- Use case 1
- Use case 2
- Use case 3

## Instructions

Step-by-step guidance on how to use the skill.

## Examples

Example code and usage patterns.

## Best Practices

Guidelines and recommendations.
```

The frontmatter requires:
- `name` - A unique identifier for your skill (lowercase, hyphens for spaces)
- `description` - A complete description of what the skill does and when to use it
- `license` - (Optional) License for the skill content

The markdown content below contains the instructions, examples, and guidelines that Claude will follow. For more details, see [How to create custom skills](https://support.claude.com/en/articles/12512198-creating-custom-skills).

## Contributing

Contributions are welcome! If you have a Testcontainers-related skill that would benefit the community:

1. Fork this repository
2. Create a new directory for your skill
3. Add a `SKILL.md` file with proper frontmatter and instructions
4. Submit a pull request with a clear description of the skill

Please ensure your skill:
- Follows the structure and format of existing skills
- Includes clear documentation and examples
- Is specific to Testcontainers or container-based testing
- Is licensed under MIT or a compatible open source license

## License

This repository is licensed under the MIT License. See the [LICENSE](./LICENSE) file for details.

## Related Projects

- [Testcontainers for Go](https://github.com/testcontainers/testcontainers-go) - The main Testcontainers for Go library
- [Testcontainers](https://testcontainers.com/) - Official Testcontainers website
- [Anthropic Skills](https://github.com/anthropics/skills) - Main skills repository with additional examples

## Acknowledgments

This repository is inspired by and follows the structure of the [Anthropic Skills repository](https://github.com/anthropics/skills). The testcontainers-go skill was originally contributed to the main skills repository in [PR #73](https://github.com/anthropics/skills/pull/73) and is maintained here as a Testcontainers-focused collection.
