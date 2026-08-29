locals {
  repo = var.github_repository
}

resource "github_repository" "this" {
  name = local.repo

  allow_merge_commit     = false
  allow_squash_merge     = true
  allow_rebase_merge     = false
  allow_auto_merge       = false
  allow_update_branch    = true
  delete_branch_on_merge = true

  squash_merge_commit_title   = "PR_TITLE"
  squash_merge_commit_message = "PR_BODY"

  lifecycle {
    prevent_destroy = true
    ignore_changes = [
      description,
      homepage_url,
      visibility,
      has_issues,
      has_projects,
      has_wiki,
      has_downloads,
      has_discussions,
      is_template,
      archived,
      topics,
      pages,
      security_and_analysis,
      vulnerability_alerts,
      web_commit_signoff_required,
      auto_init,
      gitignore_template,
      license_template,
      default_branch,
      merge_commit_title,
      merge_commit_message,
      template,
    ]
  }
}

resource "github_repository_ruleset" "main" {
  name        = "main-protection"
  repository  = github_repository.this.name
  target      = "branch"
  enforcement = "active"

  conditions {
    ref_name {
      include = ["~DEFAULT_BRANCH"]
      exclude = []
    }
  }

  rules {
    non_fast_forward = true

    pull_request {
      required_approving_review_count = 0
      require_code_owner_review       = false
      require_last_push_approval      = false
      required_review_thread_resolution = false
      dismiss_stale_reviews_on_push   = false
      allowed_merge_methods           = ["squash"]
    }

    required_status_checks {
      strict_required_status_checks_policy = true

      dynamic "required_check" {
        for_each = var.required_status_check_contexts
        content {
          context = required_check.value
        }
      }
    }
  }
}

resource "github_workflow_repository_permissions" "this" {
  repository                         = github_repository.this.name
  default_workflow_permissions       = "read"
  can_approve_pull_request_reviews   = true
}

resource "github_repository_collaborators" "this" {
  repository = github_repository.this.name

  user {
    username   = var.github_owner
    permission = "admin"
  }

  dynamic "user" {
    for_each = toset(var.maintainer_usernames)
    content {
      username   = user.value
      permission = "push"
    }
  }
}
