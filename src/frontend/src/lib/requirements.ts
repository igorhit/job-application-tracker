export function parseRequirementsInput(input: string) {
  return input
    .split('\n')
    .map(line => line.trim())
    .filter(Boolean)
}

export function stringifyRequirements(requirements: Array<{ content: string }>) {
  return requirements
    .map(requirement => requirement.content.trim())
    .filter(Boolean)
    .join('\n')
}
