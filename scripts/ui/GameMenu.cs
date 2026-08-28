using Game.Core;
using Godot;

namespace Game.UI;

public partial class GameMenu : CanvasLayer
{
    [Export] public Control Root;
    [Export] public Control ItemsPanel;
    [Export] public Control PokemonPanel;
    [Export] public Control SavePanel;
    [Export] public OptionButton CategoryFilter;
    [Export] public ItemList ItemList;
    [Export] public Label ItemDescription;
    [Export] public Label StatusLabel;
    [Export] public Button UseButton;

    private bool wasPaused;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Root.Visible = false;

        CategoryFilter.AddItem("All items");
        CategoryFilter.AddItem("General");
        CategoryFilter.AddItem("Status items");
        CategoryFilter.AddItem("Field actions");
        CategoryFilter.AddItem("TMs");

        CategoryFilter.ItemSelected += _ => RefreshItems();
        ItemList.ItemSelected += ShowSelectedItem;
        UseButton.Pressed += UseSelectedItem;
        Signals.Instance.InventoryChanged += RefreshItems;

        ShowSection(ItemsPanel);
        RefreshItems();
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (!inputEvent.IsActionPressed("menu"))
            return;

        if (Root.Visible)
            Close();
        else if (!MessageManager.IsReading())
            Open();

        GetViewport().SetInputAsHandled();
    }

    public void Open()
    {
        wasPaused = GetTree().Paused;
        Root.Visible = true;
        GetTree().Paused = true;
        RefreshItems();
    }

    public void Close()
    {
        Root.Visible = false;
        GetTree().Paused = wasPaused;
    }

    public void ShowItems() => ShowSection(ItemsPanel);
    public void ShowPokemon() => ShowSection(PokemonPanel);
    public void ShowSave() => ShowSection(SavePanel);

    private void ShowSection(Control section)
    {
        ItemsPanel.Visible = section == ItemsPanel;
        PokemonPanel.Visible = section == PokemonPanel;
        SavePanel.Visible = section == SavePanel;
        StatusLabel.Text = string.Empty;
    }

    private void RefreshItems()
    {
        if (ItemList == null)
            return;

        ItemList.Clear();
        ItemDescription.Text = "Select an item to see its description.";
        UseButton.Disabled = true;

        ItemCategory? category = CategoryFilter.Selected switch
        {
            1 => ItemCategory.General,
            2 => ItemCategory.Status,
            3 => ItemCategory.FieldAction,
            4 => ItemCategory.TechnicalMachine,
            _ => null
        };

        foreach (Inventory.ItemStack stack in Inventory.GetItems(category))
        {
            int index = ItemList.AddItem($"{stack.Item.DisplayName}  x{stack.Quantity}", stack.Item.Icon);
            ItemList.SetItemMetadata(index, stack.Item.Id);
        }

        if (ItemList.ItemCount == 0)
            ItemDescription.Text = "There are no items in this category.";
    }

    private void ShowSelectedItem(long index)
    {
        string itemId = ItemList.GetItemMetadata((int)index).AsString();
        foreach (Inventory.ItemStack stack in Inventory.GetItems())
        {
            if (stack.Item.Id != itemId)
                continue;

            ItemDescription.Text = string.IsNullOrWhiteSpace(stack.Item.Description)
                ? "No description available."
                : stack.Item.Description;
            UseButton.Disabled = false;
            return;
        }
    }

    private void UseSelectedItem()
    {
        int[] selected = ItemList.GetSelectedItems();
        if (selected.Length == 0)
            return;

        string itemId = ItemList.GetItemMetadata(selected[0]).AsString();
        Inventory.UseItem(itemId, GameManager.GetPlayer());
        StatusLabel.Text = "Item effects are not implemented yet.";
    }
}
