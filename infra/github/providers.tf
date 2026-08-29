provider "github" {
  owner = var.github_owner
  # Auth: GH_TOKEN or GITHUB_TOKEN in the environment (exported by Makefile).
}
