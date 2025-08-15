// <copyright file="ScopeConfigurationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IdentityService.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using IdentityService.Models.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service implementation for managing API scope configurations using Entity Framework.
/// Provides CRUD operations with comprehensive logging.
/// </summary>
public class ScopeConfigurationService : IScopeConfigurationService
{
    private readonly ConfigurationDbContext context;
    private readonly ILogger<ScopeConfigurationService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScopeConfigurationService"/> class.
    /// </summary>
    /// <param name="context">The configuration database context.</param>
    /// <param name="logger">The logger instance.</param>
    public ScopeConfigurationService(ConfigurationDbContext context, ILogger<ScopeConfigurationService> logger)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<ApiScopeDto?> GetByNameAsync(string scopeName, CancellationToken cancellationToken = default)
    {
        using var scope = this.logger.BeginScope("ScopeName:{ScopeName}", scopeName);

        try
        {
            this.logger.LogDebug("Retrieving API scope configuration");

            var apiScope = await this.context.ApiScopes
                .Include(s => s.UserClaims)
                .Include(s => s.Properties)
                .FirstOrDefaultAsync(s => s.Name == scopeName, cancellationToken);

            if (apiScope == null)
            {
                this.logger.LogWarning("API scope not found");
                return null;
            }

            var dto = MapToDto(apiScope);
            this.logger.LogDebug("API scope configuration retrieved successfully");
            return dto;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving API scope configuration");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ApiScopeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            this.logger.LogDebug("Retrieving all API scope configurations");

            var apiScopes = await this.context.ApiScopes
                .Include(s => s.UserClaims)
                .Include(s => s.Properties)
                .ToListAsync(cancellationToken);

            var dtos = apiScopes.Select(MapToDto).ToList();

            this.logger.LogDebug("Retrieved {Count} API scope configurations", dtos.Count);
            return dtos;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving all API scope configurations");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ApiScopeDto> CreateAsync(ApiScopeDto scope, CancellationToken cancellationToken = default)
    {
        using var logScope = this.logger.BeginScope("ScopeName:{ScopeName}", scope.Name);

        try
        {
            this.logger.LogDebug("Creating new API scope configuration");

            var entity = MapToEntity(scope);
            entity.Created = DateTime.UtcNow;

            this.context.ApiScopes.Add(entity);
            await this.context.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(entity);
            this.logger.LogInformation("API scope configuration created successfully");
            return dto;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating API scope configuration");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ApiScopeDto?> UpdateAsync(string scopeName, ApiScopeDto scope, CancellationToken cancellationToken = default)
    {
        using var logScope = this.logger.BeginScope("ScopeName:{ScopeName}", scopeName);

        try
        {
            this.logger.LogDebug("Updating API scope configuration");

            var existing = await this.context.ApiScopes
                .Include(s => s.UserClaims)
                .Include(s => s.Properties)
                .FirstOrDefaultAsync(s => s.Name == scopeName, cancellationToken);

            if (existing == null)
            {
                this.logger.LogWarning("API scope not found for update");
                return null;
            }

            UpdateEntityFromDto(existing, scope);
            existing.Updated = DateTime.UtcNow;

            await this.context.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(existing);
            this.logger.LogInformation("API scope configuration updated successfully");
            return dto;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating API scope configuration");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string scopeName, CancellationToken cancellationToken = default)
    {
        using var logScope = this.logger.BeginScope("ScopeName:{ScopeName}", scopeName);

        try
        {
            this.logger.LogDebug("Deleting API scope configuration");

            var apiScope = await this.context.ApiScopes
                .FirstOrDefaultAsync(s => s.Name == scopeName, cancellationToken);

            if (apiScope == null)
            {
                this.logger.LogWarning("API scope not found for deletion");
                return false;
            }

            this.context.ApiScopes.Remove(apiScope);
            await this.context.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation("API scope configuration deleted successfully");
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting API scope configuration");
            throw;
        }
    }

    private static ApiScopeDto MapToDto(ApiScope entity)
    {
        return new ApiScopeDto
        {
            Name = entity.Name,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
            Required = entity.Required,
            Emphasize = entity.Emphasize,
            ShowInDiscoveryDocument = entity.ShowInDiscoveryDocument,
            UserClaims = entity.UserClaims?.Select(c => c.Type).ToArray() ?? [],
            Properties = entity.Properties?.ToDictionary(p => p.Key, p => p.Value) ?? new Dictionary<string, string>(),
            Enabled = entity.Enabled,
        };
    }

    private static ApiScope MapToEntity(ApiScopeDto dto)
    {
        return new ApiScope
        {
            Name = dto.Name,
            DisplayName = dto.DisplayName,
            Description = dto.Description,
            Required = dto.Required,
            Emphasize = dto.Emphasize,
            ShowInDiscoveryDocument = dto.ShowInDiscoveryDocument,
            UserClaims = dto.UserClaims?.Select(c => new ApiScopeClaim { Type = c }).ToList() ?? [],
            Properties = dto.Properties?.Select(p => new ApiScopeProperty { Key = p.Key, Value = p.Value }).ToList() ?? [],
            Enabled = dto.Enabled,
        };
    }

    private static void UpdateEntityFromDto(ApiScope entity, ApiScopeDto dto)
    {
        entity.DisplayName = dto.DisplayName;
        entity.Description = dto.Description;
        entity.Required = dto.Required;
        entity.Emphasize = dto.Emphasize;
        entity.ShowInDiscoveryDocument = dto.ShowInDiscoveryDocument;
        entity.Enabled = dto.Enabled;

        // Update collections (simplified approach - clear and add)
        entity.UserClaims?.Clear();
        if (dto.UserClaims != null)
        {
            foreach (var claim in dto.UserClaims)
            {
                entity.UserClaims?.Add(new ApiScopeClaim { Type = claim });
            }
        }

        entity.Properties?.Clear();
        if (dto.Properties != null)
        {
            foreach (var property in dto.Properties)
            {
                entity.Properties?.Add(new ApiScopeProperty { Key = property.Key, Value = property.Value });
            }
        }
    }
}
