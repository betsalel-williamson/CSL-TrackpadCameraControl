# Idempotent GitHub project controls (OpenTofu).
# Usage: make check | make apply | make status
# Auth: uses `gh auth token` → GH_TOKEN

SHELL := /bin/bash
.SHELLFLAGS := -eu -o pipefail -c

TF_DIR := $(abspath $(dir $(lastword $(MAKEFILE_LIST))))
REPO_NAME ?= CSL-TrackpadCameraControl
OWNER ?= betsalel-williamson
APPLY_AUTO ?= 0

.DEFAULT_GOAL := check

.PHONY: check apply status preflight init import-if-needed plan stacks-side-checks

preflight:
	@command -v tofu >/dev/null || { echo "error: OpenTofu (tofu) not found on PATH"; exit 1; }
	@command -v gh >/dev/null || { echo "error: GitHub CLI (gh) not found on PATH"; exit 1; }
	@gh auth status >/dev/null
	@test -n "$$(gh auth token)" || { echo "error: gh auth token produced empty token"; exit 1; }

init: preflight
	@cd "$(TF_DIR)" && GH_TOKEN="$$(gh auth token)" tofu init -input=false

import-if-needed: init
	@cd "$(TF_DIR)" && export GH_TOKEN="$$(gh auth token)" && \
	  if ! tofu state list 2>/dev/null | grep -qx 'github_repository.this'; then \
	    echo "==> importing github_repository.this"; \
	    tofu import -input=false github_repository.this "$(REPO_NAME)"; \
	  fi && \
	  if ! tofu state list 2>/dev/null | grep -qx 'github_workflow_repository_permissions.this'; then \
	    echo "==> importing github_workflow_repository_permissions.this"; \
	    tofu import -input=false github_workflow_repository_permissions.this "$(REPO_NAME)" || true; \
	  fi && \
	  if ! tofu state list 2>/dev/null | grep -qx 'github_repository_collaborators.this'; then \
	    echo "==> importing github_repository_collaborators.this"; \
	    tofu import -input=false github_repository_collaborators.this "$(REPO_NAME)" || true; \
	  fi

plan: import-if-needed
	@cd "$(TF_DIR)" && export GH_TOKEN="$$(gh auth token)" && \
	  set +e && tofu plan -input=false -detailed-exitcode; \
	  ec=$$?; \
	  set -e; \
	  if [[ $$ec -eq 0 ]]; then echo "==> OpenTofu: in sync"; exit 0; fi; \
	  if [[ $$ec -eq 2 ]]; then echo "==> OpenTofu: DRIFT — run: make apply"; exit 2; fi; \
	  exit $$ec

stacks-side-checks: preflight
	@echo "==> stacks API"
	@gh api -H "X-GitHub-Api-Version: 2026-03-10" "repos/$(OWNER)/$(REPO_NAME)/stacks" >/dev/null \
	  && echo "    stacks: ok" \
	  || echo "    stacks: unavailable (preview may not be enabled yet)"
	@if ! gh extension list 2>/dev/null | grep -q 'github/gh-stack'; then \
	    echo "==> installing gh-stack extension"; \
	    gh extension install github/gh-stack; \
	  else \
	    echo "==> gh-stack extension: present"; \
	  fi
	@echo "==> reminder: player distribution is Steam Workshop (not npm); Changesets only bumps version + CHANGELOG"


check: plan stacks-side-checks
	@echo "==> check ok"

apply: import-if-needed
	@cd "$(TF_DIR)" && export GH_TOKEN="$$(gh auth token)" && \
	  if [[ "$(APPLY_AUTO)" == "1" ]]; then \
	    tofu apply -input=false -auto-approve; \
	  else \
	    tofu apply -input=false; \
	  fi
	@$(MAKE) stacks-side-checks
	@echo "==> apply done — re-run make check to confirm sync"

status: preflight
	@echo "==> repository merge settings"
	@gh api "repos/$(OWNER)/$(REPO_NAME)" --jq '{allow_squash_merge,allow_merge_commit,allow_rebase_merge,allow_update_branch,delete_branch_on_merge,squash_merge_commit_title,squash_merge_commit_message}'
	@echo "==> actions workflow permissions"
	@gh api "repos/$(OWNER)/$(REPO_NAME)/actions/permissions/workflow"
	@echo "==> rulesets"
	@gh api "repos/$(OWNER)/$(REPO_NAME)/rulesets" --jq '.'
	@echo "==> collaborators (login:role)"
	@gh api "repos/$(OWNER)/$(REPO_NAME)/collaborators?affiliation=direct" --jq '.[] | "\(.login):\(.role_name)"' || true
	@echo "==> environments"
	@gh api "repos/$(OWNER)/$(REPO_NAME)/environments" --jq '.environments[].name' || true
	@echo "==> stacks"
	@gh api -H "X-GitHub-Api-Version: 2026-03-10" "repos/$(OWNER)/$(REPO_NAME)/stacks" || true
