using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LifeCraft.UI
{
    public class AvatarPanel : MonoBehaviour
    {
        public GameObject hairCategoryPanel;
        public GameObject faceCategoryPanel;
        public GameObject outfitCategoryPanel;
        public GameObject accessoryCategoryPanel;

        public Transform hairContent;
        public Transform faceContent;
        public Transform outfitContent;
        public Transform accessoryContent;

        public GameObject avatarItemPrefab;
        public Button closeButton;

        public Button claimStartersButton;

        private Services.AvatarManager.AvailableAvatarsResponse currentData;
        private string currentCategory = "HAIR";
        private GameObject currentPanel;

        void Start()
        {
            closeButton.onClick.AddListener(HidePanel);
            claimStartersButton.onClick.AddListener(OnClaimStarters);

            currentPanel = hairCategoryPanel;
            currentPanel.SetActive(true);
            faceCategoryPanel.SetActive(false);
            outfitCategoryPanel.SetActive(false);
            accessoryCategoryPanel.SetActive(false);

            Services.AvatarManager.Instance.LoadAvatars();
        }

        void HidePanel()
        {
            gameObject.SetActive(false);
        }

        void OnClaimStarters()
        {
            Services.AvatarManager.Instance.StartCoroutine(
                Services.AvatarManager.Instance.ClaimStarters()
            );
        }

        public void UpdateAvatarUI(Services.AvatarManager.AvailableAvatarsResponse data)
        {
            currentData = data;
            LoadCategory(currentCategory);
            UpdateClaimButton();
        }

        private void LoadCategory(string category)
        {
            currentCategory = category;

            Transform contentTarget;
            switch (category)
            {
                case "HAIR":
                    contentTarget = hairContent;
                    currentPanel = hairCategoryPanel;
                    break;
                case "FACE":
                    contentTarget = faceContent;
                    currentPanel = faceCategoryPanel;
                    break;
                case "OUTFIT":
                    contentTarget = outfitContent;
                    currentPanel = outfitCategoryPanel;
                    break;
                case "ACCESSORY":
                    contentTarget = accessoryContent;
                    currentPanel = accessoryCategoryPanel;
                    break;
            }

            foreach (Transform child in contentTarget)
            {
                Destroy(child.gameObject);
            }

            var items = data.categories[category];
            if (items == null) return;

            foreach (var item in items)
            {
                var itemObj = Instantiate(avatarItemPrefab, contentTarget);
                var avatarItem = itemObj.GetComponent<AvatarItemUI>();
                avatarItem.Initialize(item, data.inventory);
            }
        }

        public void SwitchCategory(string category)
        {
            if (currentCategory == category) return;

            hairCategoryPanel.SetActive(false);
            faceCategoryPanel.SetActive(false);
            outfitCategoryPanel.SetActive(false);
            accessoryCategoryPanel.SetActive(false);

            switch (category)
            {
                case "HAIR":
                    hairCategoryPanel.SetActive(true);
                    break;
                case "FACE":
                    faceCategoryPanel.SetActive(true);
                    break;
                case "OUTFIT":
                    outfitCategoryPanel.SetActive(true);
                    break;
                case "ACCESSORY":
                    accessoryCategoryPanel.SetActive(true);
                    break;
            }

            LoadCategory(category);
        }

        public void UpdateEquippedAvatar(
            Services.AvatarManager.CurrentAvatar equipped,
            string category
        )
        {
            if (currentData == null) return;

            foreach (Transform child in GetContentForCategory(category))
            {
                var avatarItem = child.GetComponent<AvatarItemUI>();
                if (avatarItem != null)
                {
                    avatarItem.UpdateEquippedStatus(equipped);
                }
            }
        }

        private Transform GetContentForCategory(string category)
        {
            switch (category)
            {
                case "HAIR":
                    return hairContent;
                case "FACE":
                    return faceContent;
                case "OUTFIT":
                    return outfitContent;
                case "ACCESSORY":
                    return accessoryContent;
                default:
                    return hairContent;
            }
        }

        private void UpdateClaimButton()
        {
            if (currentData?.inventory == null) return;

            bool hasStarters = currentData.inventory.Exists(
                item => item.price == 0
            );

            claimStartersButton.interactable = !hasStarters;
        }
    }

    public class AvatarItemUI : MonoBehaviour
    {
        public Image avatarImage;
        public Text nameText;
        public Text priceText;
        public Button equipButton;
        public Button purchaseButton;
        public GameObject equippedBadge;
        public GameObject ownedBadge;

        private Services.AvatarManager.AvatarItem itemData;
        private List<Services.AvatarManager.PlayerInventoryItem> inventory;

        public void Initialize(
            Services.AvatarManager.AvatarItem item,
            List<Services.AvatarManager.PlayerInventoryItem> playerInventory
        )
        {
            itemData = item;
            inventory = playerInventory;

            nameText.text = item.name;

            bool isOwned = inventory.Exists(
                inv => inv.avatarItemId == item.id
            );

            ownedBadge.SetActive(isOwned);

            if (isOwned)
            {
                priceText.text = "OWNED";
                purchaseButton.gameObject.SetActive(false);
                equipButton.gameObject.SetActive(true);

                bool isEquipped = IsEquipped(item.id, item.category);
                equippedBadge.SetActive(isEquipped);

                if (isEquipped)
                {
                    equipButton.interactable = false;
                }
                else
                {
                    equipButton.onClick.RemoveAllListeners();
                    equipButton.onClick.AddListener(() => OnEquip());
                }
            }
            else
            {
                bool canAfford = CanAfford(item);
                priceText.text = $"{item.price} {item.priceCurrency}";
                purchaseButton.gameObject.SetActive(true);
                equipButton.gameObject.SetActive(false);
                equippedBadge.SetActive(false);

                purchaseButton.interactable = canAfford;

                if (canAfford)
                {
                    purchaseButton.onClick.RemoveAllListeners();
                    purchaseButton.onClick.AddListener(() => OnPurchase());
                }
                else
                {
                    purchaseButton.onClick.RemoveAllListeners();
                }

                ownedBadge.SetActive(false);
            }
        }

        private bool CanAfford(Services.AvatarManager.AvatarItem item)
        {
            if (item.price == 0) return true;
            if (item.priceCurrency == "game") return true;

            return Services.AvatarManager.Instance.IsOwned(item.id);
        }

        private bool IsEquipped(string avatarItemId, string category)
        {
            if (Services.AvatarManager.Instance == null) return false;
            return Services.AvatarManager.Instance.IsEquipped(avatarItemId, category);
        }

        private void OnEquip()
        {
            Services.AvatarManager.Instance.StartCoroutine(
                Services.AvatarManager.Instance.EquipAvatar(itemData.id, itemData.category)
            );
        }

        private void OnPurchase()
        {
            Services.AvatarManager.Instance.StartCoroutine(
                Services.AvatarManager.Instance.PurchaseAvatar(itemData.id)
            );
        }

        public void UpdateEquippedStatus(Services.AvatarManager.CurrentAvatar equipped)
        {
            string equippedId = GetEquippedId(equipped, itemData.category);

            bool isEquipped = equippedId == itemData.id;
            equippedBadge.SetActive(isEquipped);

            if (isEquipped)
            {
                equipButton.interactable = false;
            }
        }

        private string GetEquippedId(
            Services.AvatarManager.CurrentAvatar equipped,
            string category
        )
        {
            switch (category)
            {
                case "HAIR":
                    return equipped?.hairId;
                case "FACE":
                    return equipped?.faceId;
                case "OUTFIT":
                    return equipped?.outfitId;
                case "ACCESSORY":
                    return equipped?.accessoryId;
                default:
                    return null;
            }
        }
    }
}
