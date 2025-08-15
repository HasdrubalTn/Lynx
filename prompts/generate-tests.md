# Prompt — Generate Test Skeletons

Generate test skeletons for Issue <number> based on the approved Design document.

Instructions:
- Place in the correct test projects under `tests/`.
- Use the stack from `src/Lynx.Testing/Usings.cs`:
  - xUnit, AutoFixture, AutoFixture.AutoNSubstitute, NSubstitute, FluentAssertions.
- Mock HTTP calls via `HttpMessageHandler` or `RichardSzalay.MockHttp`.
- Ensure tests compile.
- Do NOT implement production code.
- Ask me "GO BUILD?" when done.
