using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LifeCraft.Core;

namespace LifeCraft.Services
{
    public class AvatarManager : MonoBehaviour
    {
        public static AvatarManager Instance { get; private set; }

        [System.Serializable]
        public class AvatarItem
        {
            public string id;
            public string name;
            public string category;
            public string imageUrl;
            public string rarity;
            public int price;
            public string priceCurrency;
            public bool isPremiumOnly;
        }

        [System.Serializable]
        public class PlayerInventoryItem
        {
            public string avatarItemId;
            public string name;
            public string category;
            public string imageUrl;
            public string rarity;
            public int price;
            public string priceCurrency;
            public string purchasedAt;
        }

        [System.Serializable]
        public class CurrentAvatar
        {
            public string hairId;
            public string faceId;
            public string outfitId;
            public string accessoryId;
        }

        [System.Serializable]
        public class AvailableAvatarsResponse
        {
            public Dictionary<string, List<AvatarItem>> categories;
            public List<PlayerInventoryItem> inventory;
            public CurrentAvatar equipped;
        }

        [System.Serializable]
        public class PurchaseResponse
        {
            public bool success;
            public CurrentAvatar equipped;
        }

        [System.Serializable]
        public class EquipResponse
        {
            public bool success;
            public CurrentAvatar equipped;
        }

        private AvailableAvatarsResponse currentAvatars;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public System.Collections.IEnumerator LoadAvatars()
        {
            yield return ApiClient.Instance.Get<AvailableAvatarsResponse>(
                "/api/avatars/",
                (response) =>
                {
                    currentAvatars = response;
                    UI.UIManager.Instance?.UpdateAvatarUI(response);
                }
            );
        }

        public System.Collections.IEnumerator PurchaseAvatar(string avatarItemId)
        {
            var payload = new { avatarItemId };

            yield return ApiClient.Instance.Post<PurchaseResponse>(
                "/api/avatars/purchase",
                payload,
                (response) =>
                {
                    if (response.success)
                    {
                        UI.UIManager.Instance?.ShowMessage(
                            "Avatar Purchased!",
                            "New avatar added to your inventory",
                            2f
                        );
                        LoadAvatars();
                    }
                },
                (error) =>
                {
                    UI.UIManager.Instance?.ShowError($"Failed to purchase avatar: {error}");
                }
            );
        }

        public System.Collections.IEnumerator EquipAvatar(string avatarItemId, string category)
        {
            var payload = new { avatarItemId, category };

            yield return ApiClient.Instance.Post<EquipResponse>(
                "/api/avatars/equip",
                payload,
                (response) =>
                {
                    if (response.success)
                    {
                        UI.UIManager.Instance?.UpdateEquippedAvatar(
                            response.equipped,
                            category
                        );
                        LoadAvatars();
                    }
                },
                (error) =>
                {
                    UI.UIManager.Instance?.ShowError($"Failed to equip avatar: {error}");
                }
            );
        }

        public System.Collections.IEnumerator UnequipAvatar(string category)
        {
            var payload = new { category };

            yield return ApiClient.Instance.Post<EquipResponse>(
                "/api/avatars/unequip",
                payload,
                (response) =>
                {
                    if (response.success)
                    {
                        UI.UIManager.Instance?.UpdateEquippedAvatar(
                            response.equipped,
                            category
                        );
                        LoadAvatars();
                    }
                },
                (error) =>
                {
                    UI.UIManager.Instance?.ShowError($"Failed to unequip avatar: {error}");
                }
            );
        }

        public System.Collections.IEnumerator ClaimStarters()
        {
            yield return ApiClient.Instance.Post<object>(
                "/api/avatars/claim-starters",
                null,
                (response) =>
                {
                    UI.UIManager.Instance?.ShowMessage(
                        "Starter Avatars Granted!",
                        "Free avatars added to your inventory",
                        2f
                    );
                    LoadAvatars();
                },
                (error) =>
                {
                    UI.UIManager.Instance?.ShowError($"Failed to claim starters: {error}");
                }
            );
        }

        public bool IsOwned(string avatarItemId)
        {
            if (currentAvatars == null || currentAvatars.inventory == null)
            {
                return false;
            }

            return currentAvatars.inventory.Exists(
                item => item.avatarItemId == avatarItemId
            );
        }

        public bool IsEquipped(string avatarItemId, string category)
        {
            if (currentAvatars == null || currentAvatars.equipped == null)
            {
                return false;
            }

            switch (category)
            {
                case "HAIR":
                    return currentAvatars.equipped.hairId == avatarItemId;
                case "FACE":
                    return currentAvatars.equipped.faceId == avatarItemId;
                case "OUTFIT":
                    return currentAvatars.equipped.outfitId == avatarItemId;
                case "ACCESSORY":
                    return currentAvatars.equipped.accessoryId == avatarItemId;
                default:
                    return false;
            }
        }
    }
}
