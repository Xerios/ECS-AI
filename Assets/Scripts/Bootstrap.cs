using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public struct Bootstrap : ICustomBootstrap
{
    public static World world { get => World.Active; }

    public List<Type> Initialize (List<Type> systems)
    {
        Application.targetFrameRate = 60;
        return systems;
    }
}