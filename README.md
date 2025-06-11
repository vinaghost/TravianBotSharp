Improving original code.

## Storage production fields

`UpdateStorageCommand` now saves resource production rates parsed from the `var resources` script.  `Storage` and `StorageDto` expose `ProductionWood`, `ProductionClay`, `ProductionIron` and `ProductionCrop` which represent per hour income for each resource.  These values can be used together with warehouse and granary capacity to estimate when storage will fill up, enabling more accurate NPC scheduling.
