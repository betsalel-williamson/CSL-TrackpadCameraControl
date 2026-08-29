output "repository" {
  value = github_repository.this.full_name
}

output "ruleset_id" {
  value = github_repository_ruleset.main.ruleset_id
}

output "default_workflow_permissions" {
  value = github_workflow_repository_permissions.this.default_workflow_permissions
}
