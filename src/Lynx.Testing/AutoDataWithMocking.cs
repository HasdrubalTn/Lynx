// <copyright file="AutoDataWithMocking.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Lynx.Testing;

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;

/// <summary>
/// AutoData attribute configured with NSubstitute auto-mocking.
/// This enables AutoFixture to automatically create mocks for interfaces like ILogger.
/// </summary>
public class AutoDataWithMockingAttribute : AutoDataAttribute
{
    public AutoDataWithMockingAttribute()
        : base(() => new Fixture().Customize(new AutoNSubstituteCustomization()))
    {
    }
}
