// <copyright file="ClientConfigurationService.cs" company="PlaceholderCompany">
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
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityService.Models.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service implementation for managing OAuth client configurations using Entity Framework.
/// Provides CRUD operations with DPoP support and comprehensive logging.
/// </summary>
public class ClientConfigurationService : IClientConfigurationService
{
    private readonly ConfigurationDbContext context;
    private readonly ILogger<ClientConfigurationService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientConfigurationService"/> class.
    /// </summary>
    /// <param name="context">The configuration database context.</param>
    /// <param name="logger">The logger instance.</param>
    public ClientConfigurationService(ConfigurationDbContext context, ILogger<ClientConfigurationService> logger)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<ClientDto?> GetByIdAsync(string clientId, CancellationToken cancellationToken = default)
    {
        using var scope = this.logger.BeginScope("ClientId:{ClientId}", clientId);

        try
        {
            this.logger.LogDebug("Retrieving client configuration");

            var client = await this.context.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.AllowedScopes)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.AllowedCorsOrigins)
                .FirstOrDefaultAsync(c => c.ClientId == clientId, cancellationToken);

            if (client == null)
            {
                this.logger.LogWarning("Client not found");
                return null;
            }

            var dto = MapToDto(client);
            this.logger.LogDebug("Client configuration retrieved successfully");
            return dto;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving client configuration");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ClientDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            this.logger.LogDebug("Retrieving all client configurations");

            var clients = await this.context.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.AllowedScopes)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.AllowedCorsOrigins)
                .ToListAsync(cancellationToken);

            var dtos = clients.Select(MapToDto).ToList();

            this.logger.LogDebug("Retrieved {Count} client configurations", dtos.Count);
            return dtos;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving all client configurations");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ClientDto> CreateAsync(ClientDto client, CancellationToken cancellationToken = default)
    {
        using var scope = this.logger.BeginScope("ClientId:{ClientId}", client.Id);

        try
        {
            this.logger.LogDebug("Creating new client configuration");

            var entity = MapToEntity(client);
            entity.Created = DateTime.UtcNow;

            this.context.Clients.Add(entity);
            await this.context.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(entity);
            this.logger.LogInformation("Client configuration created successfully");
            return dto;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating client configuration");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ClientDto?> UpdateAsync(string clientId, ClientDto client, CancellationToken cancellationToken = default)
    {
        using var scope = this.logger.BeginScope("ClientId:{ClientId}", clientId);

        try
        {
            this.logger.LogDebug("Updating client configuration");

            var existing = await this.context.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.AllowedScopes)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.AllowedCorsOrigins)
                .FirstOrDefaultAsync(c => c.ClientId == clientId, cancellationToken);

            if (existing == null)
            {
                this.logger.LogWarning("Client not found for update");
                return null;
            }

            UpdateEntityFromDto(existing, client);
            existing.Updated = DateTime.UtcNow;

            await this.context.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(existing);
            this.logger.LogInformation("Client configuration updated successfully");
            return dto;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating client configuration");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string clientId, CancellationToken cancellationToken = default)
    {
        using var scope = this.logger.BeginScope("ClientId:{ClientId}", clientId);

        try
        {
            this.logger.LogDebug("Deleting client configuration");

            var client = await this.context.Clients
                .FirstOrDefaultAsync(c => c.ClientId == clientId, cancellationToken);

            if (client == null)
            {
                this.logger.LogWarning("Client not found for deletion");
                return false;
            }

            this.context.Clients.Remove(client);
            await this.context.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation("Client configuration deleted successfully");
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting client configuration");
            throw;
        }
    }

    private static ClientDto MapToDto(Client entity)
    {
        return new ClientDto
        {
            Id = entity.ClientId,
            ClientName = entity.ClientName,
            Description = entity.Description,
            RequireDPoP = entity.RequireDPoP,
            AllowedGrantTypes = entity.AllowedGrantTypes?.Select(gt => gt.GrantType).ToArray() ?? [],
            AllowedScopes = entity.AllowedScopes?.Select(s => s.Scope).ToArray() ?? [],
            RedirectUris = entity.RedirectUris?.Select(ru => ru.RedirectUri).ToArray() ?? [],
            PostLogoutRedirectUris = entity.PostLogoutRedirectUris?.Select(plru => plru.PostLogoutRedirectUri).ToArray() ?? [],
            AllowedCorsOrigins = entity.AllowedCorsOrigins?.Select(co => co.Origin).ToArray() ?? [],
            Enabled = entity.Enabled,
            RequireClientSecret = entity.RequireClientSecret,
            RequirePkce = entity.RequirePkce,
            AllowPlainTextPkce = entity.AllowPlainTextPkce,
            RequireRequestObject = entity.RequireRequestObject,
            AllowAccessTokensViaBrowser = entity.AllowAccessTokensViaBrowser,
            AccessTokenLifetime = entity.AccessTokenLifetime,
            RefreshTokenUsage = entity.RefreshTokenUsage,
            RefreshTokenExpiration = entity.RefreshTokenExpiration,
            AbsoluteRefreshTokenLifetime = entity.AbsoluteRefreshTokenLifetime,
            SlidingRefreshTokenLifetime = entity.SlidingRefreshTokenLifetime,
        };
    }

    private static Client MapToEntity(ClientDto dto)
    {
        return new Client
        {
            ClientId = dto.Id,
            ClientName = dto.ClientName,
            Description = dto.Description,
            RequireDPoP = dto.RequireDPoP,
            AllowedGrantTypes = dto.AllowedGrantTypes?.Select(gt => new ClientGrantType { GrantType = gt }).ToList() ?? [],
            AllowedScopes = dto.AllowedScopes?.Select(s => new ClientScope { Scope = s }).ToList() ?? [],
            RedirectUris = dto.RedirectUris?.Select(ru => new ClientRedirectUri { RedirectUri = ru }).ToList() ?? [],
            PostLogoutRedirectUris = dto.PostLogoutRedirectUris?.Select(plru => new ClientPostLogoutRedirectUri { PostLogoutRedirectUri = plru }).ToList() ?? [],
            AllowedCorsOrigins = dto.AllowedCorsOrigins?.Select(co => new ClientCorsOrigin { Origin = co }).ToList() ?? [],
            Enabled = dto.Enabled,
            RequireClientSecret = dto.RequireClientSecret,
            RequirePkce = dto.RequirePkce,
            AllowPlainTextPkce = dto.AllowPlainTextPkce,
            RequireRequestObject = dto.RequireRequestObject,
            AllowAccessTokensViaBrowser = dto.AllowAccessTokensViaBrowser,
            AccessTokenLifetime = dto.AccessTokenLifetime,
            RefreshTokenUsage = dto.RefreshTokenUsage,
            RefreshTokenExpiration = dto.RefreshTokenExpiration,
            AbsoluteRefreshTokenLifetime = dto.AbsoluteRefreshTokenLifetime,
            SlidingRefreshTokenLifetime = dto.SlidingRefreshTokenLifetime,
        };
    }

    private static void UpdateEntityFromDto(Client entity, ClientDto dto)
    {
        entity.ClientName = dto.ClientName;
        entity.Description = dto.Description;
        entity.RequireDPoP = dto.RequireDPoP;
        entity.Enabled = dto.Enabled;
        entity.RequireClientSecret = dto.RequireClientSecret;
        entity.RequirePkce = dto.RequirePkce;
        entity.AllowPlainTextPkce = dto.AllowPlainTextPkce;
        entity.RequireRequestObject = dto.RequireRequestObject;
        entity.AllowAccessTokensViaBrowser = dto.AllowAccessTokensViaBrowser;
        entity.AccessTokenLifetime = dto.AccessTokenLifetime;
        entity.RefreshTokenUsage = dto.RefreshTokenUsage;
        entity.RefreshTokenExpiration = dto.RefreshTokenExpiration;
        entity.AbsoluteRefreshTokenLifetime = dto.AbsoluteRefreshTokenLifetime;
        entity.SlidingRefreshTokenLifetime = dto.SlidingRefreshTokenLifetime;

        // Update collections (simplified approach - clear and add)
        entity.AllowedGrantTypes?.Clear();
        if (dto.AllowedGrantTypes != null)
        {
            foreach (var grantType in dto.AllowedGrantTypes)
            {
                entity.AllowedGrantTypes?.Add(new ClientGrantType { GrantType = grantType });
            }
        }

        entity.AllowedScopes?.Clear();
        if (dto.AllowedScopes != null)
        {
            foreach (var scope in dto.AllowedScopes)
            {
                entity.AllowedScopes?.Add(new ClientScope { Scope = scope });
            }
        }

        entity.RedirectUris?.Clear();
        if (dto.RedirectUris != null)
        {
            foreach (var uri in dto.RedirectUris)
            {
                entity.RedirectUris?.Add(new ClientRedirectUri { RedirectUri = uri });
            }
        }

        entity.PostLogoutRedirectUris?.Clear();
        if (dto.PostLogoutRedirectUris != null)
        {
            foreach (var uri in dto.PostLogoutRedirectUris)
            {
                entity.PostLogoutRedirectUris?.Add(new ClientPostLogoutRedirectUri { PostLogoutRedirectUri = uri });
            }
        }

        entity.AllowedCorsOrigins?.Clear();
        if (dto.AllowedCorsOrigins != null)
        {
            foreach (var origin in dto.AllowedCorsOrigins)
            {
                entity.AllowedCorsOrigins?.Add(new ClientCorsOrigin { Origin = origin });
            }
        }
    }
}
