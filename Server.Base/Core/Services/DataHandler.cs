﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Models;
using Server.Base.Database.Abstractions;

namespace Server.Base.Core.Services;

public abstract class DataHandler<Entity, Database>(IServiceProvider services) : IService where Entity : PersistantData where Database : BaseDataContext
{
    public readonly ILogger<Entity> Logger = services.GetRequiredService<ILogger<Entity>>();
    public readonly EventSink Sink = services.GetRequiredService<EventSink>();
    public readonly IServiceProvider Services = services;

    public abstract bool HasDefault { get; }

    public virtual void Initialize()
    {
        Sink.WorldLoad += Load;
        Sink.CreateData += () => CreateDefault($"new {typeof(Entity).Name.ToLower()}");
    }

    public void Load()
    {
        if (GetCount() <= 0)
            CreateDefault("server owner");
    }

    public abstract Entity CreateDefault();

    public void CreateDefault(string name)
    {
        if (!HasDefault)
            return;

        var type = typeof(Entity).Name.ToLower();

        Logger.LogDebug("This server does not have a(n) {Type}.", type);
        Logger.LogDebug("Please create a(n) {Type} for the {Name} now", type, name);

        var t = CreateDefault();

        if (t == null)
        {
            Logger.LogError("Failed to create {Type}.", type);
        }
        else
        {
            Add(t);
            Logger.LogDebug("Created {Name}.", type);
        }
    }

    public int CreateNewId() => GetCount() == 0 ? 1 : GetMax() + 1;

    public void Add(Entity entity, int id = -1)
    {
        if (id < 0)
            id = CreateNewId();

        if (entity is PersistantData pd)
            pd.Id = id;

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Database>();

        lock (db.Lock)
        {
            db.Set<Entity>().Add(entity);

            db.SaveChanges();
        }
    }

    public void Remove(int id)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Database>();

        lock (db.Lock)
        {
            var set = db.Set<Entity>();

            var dbEntry = set.Find(id);

            if (dbEntry != null)
                set.Remove(dbEntry);

            db.SaveChanges();
        }
    }

    protected int GetCount()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Database>();

        lock (db.Lock)
        {
            return db.Set<Entity>().Count();
        }
    }

    protected int GetMax()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Database>();

        lock (db.Lock)
        {
            return db.Set<Entity>().Max(a => a.Id);
        }
    }

    public void Update(Entity entity)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Database>();

        lock (db.Lock)
        {
            db.Set<Entity>().Update(entity);

            db.SaveChanges();
        }
    }

    public Entity Get(int id)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Database>();

        lock (db.Lock)
        {
            var entity = db.Set<Entity>().Find(id);

            db.Entry(entity).State = EntityState.Detached;

            return entity;
        }
    }
}
