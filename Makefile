.PHONY: bootstrap hooks
bootstrap: hooks
hooks:
	@./scripts/enable-githooks.sh
