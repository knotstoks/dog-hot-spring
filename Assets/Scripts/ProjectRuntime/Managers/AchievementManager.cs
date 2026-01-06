using BroccoliBunnyStudios.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectRuntime.Managers
{
    public class AchievementManager
    {
        [Serializable]
        public struct UserAchievement
        {
            [field: SerializeField]
            [JsonProperty]
            public string AchievementId { get; set; }

            [field: SerializeField]
            [JsonProperty]
            public long Count { get; set; }

            [field: SerializeField]
            [JsonProperty]
            public long CurrentCount { get; set; }

            [field: SerializeField]
            [JsonProperty]
            public AchievementStatusEnum Status { get; set; }

            public UserAchievement(string achievementId, long count)
            {
                this.AchievementId = achievementId;
                this.Count = count;
                this.CurrentCount = 0;
                this.Status = AchievementStatusEnum.InProgress;
            }
        }

        private static readonly Lazy<AchievementManager> s_lazy = new(() => new AchievementManager());
        public static AchievementManager Instance => s_lazy.Value;

        // Acessible variables
        public ReadOnlyDictionary<string, UserAchievement> UserAchievements => this._readOnlyUserAchievements;
        public event Action<string> OnUserAchievementChange;

        // Internal variables
        private readonly Dictionary<string, UserAchievement> _userAchievements;
        private readonly ReadOnlyDictionary<string, UserAchievement> _readOnlyUserAchievements;

        private AchievementManager()
        {
            var sm = SaveManager.Instance;

            this._userAchievements = JsonConvert.DeserializeObject<Dictionary<string, UserAchievement>>(sm.UserAchievements) ?? new Dictionary<string, UserAchievement>();
            this._readOnlyUserAchievements = new(this._userAchievements);

            var toRemove = new List<string>();
            foreach (var achievementId in this._userAchievements.Keys)
            {
                if (!DAchievement.GetDataById(achievementId).HasValue)
                {
                    toRemove.Add(achievementId);
                }
            }

            if (toRemove.Count > 0)
            {
                foreach (var achId in toRemove)
                {
                    this._userAchievements.Remove(achId);
                }
                this.SaveAchievements();
            }

            this.UnlockNewAchievements();
        }

        /// <summary>
        /// Checks if any new achievements have been added and unlock them if needed
        /// </summary>
        public void UnlockNewAchievements()
        {

        }

        /// <summary>
        /// Resets all user achievements
        /// </summary>
        public void ResetAchievements()
        {

        }

        private void SaveAchievements()
        {
            SaveManager.Instance.UserAchievements = JsonConvert.SerializeObject(this._userAchievements);
        }

        #region Credit Achievements
        public void GenericAddCredit(AchievementType achievementType, long count)
        {
            if (count == 0)
            {
                return;
            }

            if (count < 0)
            {
                Debug.LogError($"Tried to give negative credit for achievement_type={achievementType} with count={count}");
                return;
            }

            var modified = false;

            // For each achievement of this type
            var dAchs = DAchievement.GetDataForAchievementType(achievementType);
            var creditType = s_creditDict[achievementType];
            foreach (var dAch in dAchs)
            {
                if (!this._userAchievements.ContainsKey(dAch.AchievementId))
                {
                    switch (creditType)
                    {
                        case AchievementCreditType.AddCount:
                            modified = this.AddCount(count, this._userAchievements[dAch.AchievementId], dAch) || modified;
                            break;
                        case AchievementCreditType.AddReach:
                            modified = this.AddReach(count, this._userAchievements[dAch.AchievementId], dAch) || modified;
                            break;
                    }
                }
            }

            if (modified)
            {
                this.SaveAchievements();
            }
        }

        private bool AddCount(long count, UserAchievement uAch, AchievementData dAch)
        {
            if (uAch.Status == AchievementStatusEnum.Unlocked)
            {
                return false;
            }

            uAch.CurrentCount += count;
            if (uAch.CurrentCount >= uAch.Count)
            {
                uAch.CurrentCount = uAch.Count;
                if (uAch.Status == AchievementStatusEnum.InProgress)
                {
                    uAch.Status = AchievementStatusEnum.Unlocked;
                    
                    // TODO: Hook up Steam Achievement

                    // TODO: Show in game achievement
                }
            }
            this._userAchievements[uAch.AchievementId] = uAch;
            this.OnUserAchievementChange?.Invoke(uAch.AchievementId);

            return true;
        }

        private bool AddReach(long count, UserAchievement uAch, AchievementData dAch)
        {
            if (uAch.Status == AchievementStatusEnum.Unlocked)
            {
                return false;
            }

            if (uAch.CurrentCount > count)
            {
                return false;
            }

            uAch.CurrentCount = count;
            if (uAch.CurrentCount >= uAch.Count)
            {
                uAch.CurrentCount = uAch.Count;
                if (uAch.Status == AchievementStatusEnum.InProgress)
                {
                    uAch.Status = AchievementStatusEnum.Unlocked;

                    // TODO: Hook up Steam Achievement

                    // TODO: Show in game achievement
                }
            }
            this._userAchievements[uAch.AchievementId] = uAch;
            this.OnUserAchievementChange?.Invoke(uAch.AchievementId);

            return true;
        }
        #endregion

        public enum AchievementStatusEnum
        {
            InProgress = 1,
            Unlocked = 2,
        }

        public enum AchievementCreditType
        {
            AddCount = 1,
            AddReach = 2,
        }

        public readonly Dictionary<AchievementType, AchievementCreditType> s_creditDict = new Dictionary<AchievementType, AchievementCreditType>
        {
            { AchievementType.LEVEL_COMPLETE, AchievementCreditType.AddReach },
        };
    }
}