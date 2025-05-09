﻿using System;
namespace DobissConnectorService.Dobiss.Models
{
    public class Light
    {
        public Light(int moduleKey, int key, ModuleType moduleType, string name)
        {
            ModuleKey = moduleKey;
            Key = key;
            ModuleType = moduleType;
            Name = name;
        }
        public int ModuleKey { get; }
        public int Key { get; }
        public ModuleType ModuleType { get; }
        public string Name { get; }
        public int CurrentValue { get; set; }
    }
}
