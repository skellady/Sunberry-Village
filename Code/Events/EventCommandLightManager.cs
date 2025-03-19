using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;

namespace SunberryVillage.Events;

public class EventCommandLightManager
{
    private Event _event;
    private Action _unregisterAction;
    private Dictionary<NPC, LightSourceWrapper> _lightmapping;

    public EventCommandLightManager(Event @event, Action unregisterAction)
    {
        _event = @event;
        _unregisterAction = unregisterAction;
        Globals.EventHelper.GameLoop.UpdateTicked += OnUpdateTicked;
        _lightmapping = new();
    }

    public void addLight(NPC actor, LightSourceWrapper lightSourceWrapper)
    {
        Game1.currentLightSources.Add(lightSourceWrapper.LightSource.Id, lightSourceWrapper.LightSource);
        _lightmapping.Add(actor, lightSourceWrapper);
    }

    public void removeLight(string lightId)
    {
        _lightmapping.RemoveWhere<NPC, LightSourceWrapper>(entry =>
        {
            if(entry.Value.LightSource.Id == lightId)
            {
                Game1.currentLightSources.Remove(lightId);
                return true;
            }
            return false;
        });
    }

    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (!Game1.eventUp || Game1.eventOver)
        {
            _unregisterAction();
            Globals.EventHelper.GameLoop.UpdateTicked -= OnUpdateTicked;
            return;
        }
        foreach(var entry in _lightmapping)
        {
            var actor = entry.Key;
            var lightSourceWrapper = entry.Value;
            lightSourceWrapper.LightSource.position.Value = actor.Position + lightSourceWrapper.Offset;
        }
    }

    public record LightSourceWrapper(LightSource LightSource, Vector2 Offset);
}