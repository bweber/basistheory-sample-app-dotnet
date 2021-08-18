MAKEFLAGS += --silent

token:
	dotnet run --project src/BasisTheory.SampleApp

release:
	./scripts/release.sh

verify:
	./scripts/verify.sh
