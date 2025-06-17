Improving original code.

## Storage production fields

`UpdateStorageCommand` now saves resource production rates parsed from the `var resources` script.  `Storage` and `StorageDto` expose `ProductionWood`, `ProductionClay`, `ProductionIron` and `ProductionCrop` which represent per hour income for each resource.  These values can be used together with warehouse and granary capacity to estimate when storage will fill up, enabling more accurate NPC scheduling.


## Auto NPC crop scheduling

When the **NPC Crop exchange** option is enabled the bot now predicts when the granary will reach the configured `AutoNPCGranaryPercent`.  Using the stored crop production and current storage values an `NpcTask` is scheduled for that time in the task list, ensuring NPC trades happen right before the granary overflows.
The trigger re-evaluates this schedule each time resource or production data is updated so existing NPC tasks automatically shift to the new predicted time.

## Post NPC upgrade check

After an NPC trade completes successfully the bot immediately triggers an upgrade building check.  Since resources have been redistributed an upgrade might now be affordable, so the building queue is evaluated again.


