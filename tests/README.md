# Tests

Automated tests are organized by implementation language.

```text
tests/
├─ dotnet/
├─ cpp/
├─ java/
└─ swift/
```

Only create language-specific test directories when there is an implementation or test harness to maintain.

Cross-language behavioral fixtures should live under `/spec` so each implementation can run equivalent conformance cases with its native test framework.
