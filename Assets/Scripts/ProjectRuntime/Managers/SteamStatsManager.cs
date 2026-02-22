using Steamworks;
using UnityEngine;

namespace ProjectRuntime.Managers
{
    public class SteamStatsManager : MonoBehaviour
    {
        public static SteamStatsManager Instance { get; private set; }

        // PlayerID
        private CSteamID m_PlayerID;

        // GameID
        private CGameID m_GameID;

        // Accessible Variables
        public int LevelProgress => this._levelProgress;

        // Private Variables
        private int _levelProgress;

        // Callbacks
        protected Callback<UserStatsReceived_t> m_UserStatsReceived;
        protected Callback<UserStatsStored_t> m_UserStatsStored;
        protected Callback<UserAchievementStored_t> m_UserAchievementStored;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("There are 2 or more SteamStatsManagers in the scene");
            }

#if !DISABLESTEAMWORKS
            if (!SteamManager.Initialized)
                return;

            // Cache the PlayerID and GameID for use in the Callbacks
            m_PlayerID = SteamUser.GetSteamID();
            m_GameID = new CGameID(SteamUtils.GetAppID());

            m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
            m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
            m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnUserAchievementStored);

            this.RefreshUserStats();
#endif
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void RefreshUserStats()
        {
            if (!SteamManager.Initialized)
            {
                return;
            }

            SteamUserStats.RequestUserStats(m_PlayerID);
        }

        private void OnUserStatsReceived(UserStatsReceived_t pCallback)
        {
            // we may get callbacks for other games' stats arriving, ignore them
            if ((ulong)m_GameID != pCallback.m_nGameID || EResult.k_EResultOK != pCallback.m_eResult)
            {
                return;
            }

            SteamUserStats.GetUserStat(m_PlayerID, "LEVEL_PROGRESS", out this._levelProgress);
        }

        private void OnUserStatsStored(UserStatsStored_t pCallback)
        {
            // we may get callbacks for other games' stats arriving, ignore them
            if ((ulong)m_GameID != pCallback.m_nGameID || EResult.k_EResultOK != pCallback.m_eResult)
            {
                return;
            }

            // Empty for now
        }

        private void OnUserAchievementStored(UserAchievementStored_t pCallback)
        {
            // we may get callbacks for other games' stats arriving, ignore them
            if ((ulong)m_GameID != pCallback.m_nGameID)
            {
                return;
            }

            // Empty for now
        }
    }
}