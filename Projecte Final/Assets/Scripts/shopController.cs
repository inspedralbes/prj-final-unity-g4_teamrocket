using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class ShopController : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public string id;
        public string name;
        public int price;
        public string description;
        public Sprite icon;
    }

    [SerializeField] private UIDocument shopUI;
    [SerializeField] private int playerMoney = 7189;
    [SerializeField] private List<ShopItem> equipmentItems;

    private VisualElement root;
    private Label moneyLabel;
    private VisualElement equipmentGrid;
    private Image itemPreview;
    private Label itemName;
    private Label itemDescription;
    private Label itemPrice;
    private Button buyButton;
    private Button sellButton;
    private Button closeButton;

    private ShopItem selectedItem;
    public System.Action<bool> OnShopToggle;

    private void OnEnable()
    {
        root = shopUI.rootVisualElement;
        root.style.display = DisplayStyle.None;

        moneyLabel = root.Q<Label>("player-money");
        equipmentGrid = root.Q<VisualElement>("equipment-grid");
        itemPreview = root.Q<Image>("item-preview");
        itemName = root.Q<Label>("item-name");
        itemDescription = root.Q<Label>("item-description");
        itemPrice = root.Q<Label>("item-price");
        buyButton = root.Q<Button>("buy-button");
        sellButton = root.Q<Button>("sell-button");
        closeButton = root.Q<Button>("close-button");

        UpdateMoneyDisplay();
        PopulateShop();

        buyButton.clicked += OnBuyClicked;
        sellButton.clicked += OnSellClicked;
        closeButton.clicked += OnCloseClicked;
    }

    private void OnDisable()
    {
        buyButton.clicked -= OnBuyClicked;
        sellButton.clicked -= OnSellClicked;
        closeButton.clicked -= OnCloseClicked;
    }

    private void PopulateShop()
    {
        equipmentGrid.Clear();

        foreach (var item in equipmentItems)
        {
            var itemElement = new VisualElement();
            itemElement.AddToClassList("equipment-item");

            var icon = new Image { sprite = item.icon };
            icon.AddToClassList("item-icon");

            var info = new VisualElement();
            info.AddToClassList("item-info");

            var nameLabel = new Label(item.name);
            nameLabel.AddToClassList("item-name");

            var priceLabel = new Label($"${item.price}");
            priceLabel.AddToClassList("item-price");

            info.Add(nameLabel);
            info.Add(priceLabel);

            itemElement.Add(icon);
            itemElement.Add(info);

            itemElement.RegisterCallback<ClickEvent>(_ => ShowItemDetails(item));

            equipmentGrid.Add(itemElement);
        }
    }

    private void ShowItemDetails(ShopItem item)
    {
        selectedItem = item;
        itemPreview.sprite = item.icon;
        itemName.text = item.name;
        itemDescription.text = item.description;
        itemPrice.text = $"${item.price}";
    }

    private void OnBuyClicked()
    {
        if (selectedItem == null) return;

        if (playerMoney >= selectedItem.price)
        {
            playerMoney -= selectedItem.price;
            UpdateMoneyDisplay();
            Debug.Log($"Bought {selectedItem.name} for ${selectedItem.price}");
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }

    private void OnSellClicked()
    {
        if (selectedItem == null) return;

        playerMoney += selectedItem.price / 2;
        UpdateMoneyDisplay();
        Debug.Log($"Sold {selectedItem.name} for ${selectedItem.price / 2}");
    }

    private void OnCloseClicked()
    {
        ToggleShop(false);
    }

    private void UpdateMoneyDisplay()
    {
        moneyLabel.text = $"${playerMoney}";
    }

    public void ToggleShop(bool show)
    {
        root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        Time.timeScale = show ? 0f : 1f;
        OnShopToggle?.Invoke(show);
    }

    public bool IsShopVisible()
    {
        return root.style.display == DisplayStyle.Flex;
    }
}