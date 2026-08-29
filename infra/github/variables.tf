variable "github_owner" {
  type        = string
  description = "GitHub user or org that owns the repository"
  default     = "betsalel-williamson"
}

variable "github_repository" {
  type        = string
  description = "Repository name (without owner)"
  default     = "CSL-TrackpadCameraControl"
}

variable "required_status_check_contexts" {
  type        = list(string)
  description = "CI check context names required before merging to main"
  default     = ["Commitlint", "Validate"]
}

variable "maintainer_usernames" {
  type        = list(string)
  description = "Non-owner users allowed Write (personal repos: permission push). Prefer empty — use forks."
  default     = []
}

variable "npm_publish_wait_timer_minutes" {
  type        = number
  description = "Optional delay before npm-publish environment jobs start (cancel window). 0 disables."
  default     = 5
}
