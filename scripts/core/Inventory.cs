using System.Collections.Generic;
using System.Linq;
using Game.Gameplay.Items;
using Godot;

namespace Game.Core;

public partial class Inventory : Node
{
    public sealed class ItemStack
    {
        public ItemDefinition Item { get; init; }
        public int Quantity { get; set; }
    }

    public static Inventory Instance { get; private set; }

    private readonly Dictionary<string, ItemStack> items = new();

    public override void _Ready()
    {
        Instance = this;
    }

    public static void AddItem(string itemId, string itemName, int quantity = 1)
    {
        AddItem(new ItemDefinition
        {
            Id = itemId,
            DisplayName = string.IsNullOrWhiteSpace(itemName) ? itemId : itemName
        }, quantity);
    }

    public static void AddItem(ItemDefinition item, int quantity = 1)
    {
        if (Instance == null || item == null || string.IsNullOrWhiteSpace(item.Id) || quantity <= 0)
            return;

        if (Instance.items.TryGetValue(item.Id, out ItemStack stack))
            stack.Quantity += quantity;
        else
            Instance.items[item.Id] = new ItemStack { Item = item, Quantity = quantity };

        Signals.EmitGlobalSignal(
            Signals.SignalName.ItemReceived,
            item.Id,
            item.DisplayName,
            quantity);
        Signals.EmitGlobalSignal(Signals.SignalName.InventoryChanged);
    }

    public static int GetItemCount(string itemId)
    {
        if (Instance == null || string.IsNullOrWhiteSpace(itemId))
            return 0;

        return Instance.items.TryGetValue(itemId, out ItemStack stack) ? stack.Quantity : 0;
    }

    public static IReadOnlyList<ItemStack> GetItems(ItemCategory? category = null)
    {
        if (Instance == null)
            return System.Array.Empty<ItemStack>();

        return Instance.items.Values
            .Where(stack => category == null || stack.Item.Category == category)
            .OrderBy(stack => stack.Item.DisplayName)
            .ToList();
    }

    public static void UseItem(string itemId, Node context = null)
    {
        if (Instance?.items.TryGetValue(itemId, out ItemStack stack) == true)
            stack.Item.Use(context);
    }
}
