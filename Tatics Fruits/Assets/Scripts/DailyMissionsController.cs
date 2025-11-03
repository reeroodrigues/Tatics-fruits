using System;
using System.Collections.Generic;
using System.Linq;
using New_GameplayCore.Views;
using UnityEngine;
using Random = UnityEngine.Random;

public class DailyMissionsController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerProfileController profile;
    [SerializeField] private List<DailyMissionSo> missionPool;
    [SerializeField, Range(1, 5)] private int missionsPerDay = 5;
    [SerializeField] private bool useLocalTime = true;

    public event Action<bool> OnAttentionChanged;
    public event Action OnDailyLoginChanged;
    public event Action OnDailyMissionsChanged;

    public bool UseLocalTime => useLocalTime;
    public DateTime GetNow() => useLocalTime ? DateTime.Now : DateTime.UtcNow;

    public static DailyMissionsController Instance { get; private set; }

    private string TodayKey => (useLocalTime ? DateTime.Now : DateTime.UtcNow).ToString("yyyyMMdd");

    public struct DailyLoginDayInfo
    {
        public int Index;
        public int Reward;
        public bool Claimed;
        public bool Claimable;
    }

    // -------------------------
    // Lifecycle
    // -------------------------
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        EnsureLoginInitialized();
        MigrateLegacyLoginIfNeeded();
        EnsureDayGenerated();
        FireAttention();
    }

    // -------------------------
    // Daily Login
    // -------------------------
    private void EnsureLoginInitialized()
    {
        if (profile.Data == null) return;
        if (profile.Data.daily == null)
            profile.Data.daily = new DailySystemData();

        var l = profile.Data.daily.login;
        if (l == null)
        {
            profile.Data.daily.login = new DailyLoginData();
            l = profile.Data.daily.login;
        }

        if (l.rewards == null || l.rewards.Count != 7)
            l.rewards = new List<int> { 20, 30, 40, 60, 80, 100, 200 };

        if (l.claimed == null || l.claimed.Count != 7)
            l.claimed = new List<bool> { false, false, false, false, false, false, false };

        l.cycleIndex = Mathf.Clamp(l.cycleIndex, 0, 6);
    }

    private void MigrateLegacyLoginIfNeeded()
    {
        var daily = profile.Data.daily;
        if (daily == null) return;

        var l = daily.login;
        if (l == null) { EnsureLoginInitialized(); l = daily.login; }

        if (!string.IsNullOrEmpty(daily.lastLoginRewardDayKey) &&
            string.IsNullOrEmpty(l.lastClaimDayKey))
        {
            l.lastClaimDayKey = daily.lastLoginRewardDayKey;
        }

        if (daily.loginRewardGold > 0 && l.rewards != null && l.rewards.Count == 7)
        {
            if (l.rewards[0] == 20) l.rewards[0] = daily.loginRewardGold;
        }

        profile.SaveProfile();
    }

    public List<DailyLoginDayInfo> GetLoginDays()
    {
        EnsureLoginInitialized();
        var l = profile.Data.daily.login;

        var list = new List<DailyLoginDayInfo>(7);
        for (int i = 0; i < 7; i++)
        {
            bool claimable = (i == l.cycleIndex) && l.lastClaimDayKey != TodayKey && !l.claimed[i];
            list.Add(new DailyLoginDayInfo
            {
                Index = i,
                Reward = l.rewards[i],
                Claimed = l.claimed[i],
                Claimable = claimable
            });
        }
        return list;
    }

    public bool TryClaimDailyLoginDay(int index)
    {
        EnsureLoginInitialized();
        var l = profile.Data.daily.login;

        if (index != l.cycleIndex) return false;
        if (l.claimed[index]) return false;
        if (l.lastClaimDayKey == TodayKey) return false;

        profile.AddGoldAndSave(l.rewards[index]);

        l.claimed[index] = true;
        l.lastClaimDayKey = TodayKey;

        l.cycleIndex++;
        if (l.cycleIndex >= 7)
        {
            l.cycleIndex = 0;
            for (int i = 0; i < 7; i++) l.claimed[i] = false;
        }

        profile.SaveProfile();

        OnDailyLoginChanged?.Invoke();
        FireAttention();
        return true;
    }

    public bool IsDailyLoginAvailable()
    {
        EnsureLoginInitialized();
        var l = profile.Data.daily.login;
        return l.lastClaimDayKey != TodayKey && !l.claimed[l.cycleIndex];
    }

    public bool TryClaimDailyLogin()
    {
        EnsureLoginInitialized();
        var l = profile.Data.daily.login;
        return TryClaimDailyLoginDay(l.cycleIndex);
    }

    // -------------------------
    // Missions (day generation / getters)
    // -------------------------
    public void EnsureDayGenerated()
    {
        if (profile == null)
        {
            Debug.LogWarning("[DailyMissionsController] ProfileController não atribuído — tentando localizar automaticamente.");
            profile = FindObjectOfType<PlayerProfileController>();
            if (profile == null)
            {
                Debug.LogError("[DailyMissionsController] Não foi possível encontrar PlayerProfileController na cena!");
                return;
            }
        }

        if (profile.Data == null)
        {
            Debug.LogWarning("[DailyMissionsController] Criando novo ProfileData pois estava nulo.");
            if (profile.Data == null)
                profile.Data = new PlayerProfileData();
        }

        if (profile.Data.daily == null)
        {
            profile.Data.daily = new DailySystemData();
        }
        
        var daily = profile.Data.daily;
        if (daily == null)
        {
            profile.Data.daily = new DailySystemData();
            daily = profile.Data.daily;
        }

        if (daily.dayKey == TodayKey && daily.missions != null && daily.missions.Count == missionsPerDay)
            return;

        daily.dayKey = TodayKey;
        daily.missions = new List<DailyMissionState>();

        var pool = missionPool.Where(m => m != null).OrderBy(_ => Random.value).ToList();
        for (int i = 0; i < Mathf.Min(missionsPerDay, pool.Count); i++)
        {
            var def = pool[i];
            var desc = def.descriptionTemplate;
            if (def.eventType == MissionEventType.WinLevel && def.levelParam > 0)
                desc = desc.Replace("{level}", def.levelParam.ToString());

            daily.missions.Add(new DailyMissionState
            {
                missionId = def.id,
                description = desc,
                progress = 0,
                target = Mathf.Max(1, def.target),
                rewardGold = Mathf.Max(0, def.rewardGold),
                completed = false,
                claimed = false
            });
        }

        profile.SaveProfile();
        OnDailyMissionsChanged?.Invoke();
        FireAttention();
    }

    public DailyMissionSo GetDefinition(string missionId)
    {
        if (string.IsNullOrEmpty(missionId)) return null;
        foreach (var def in missionPool)
            if (def != null && def.id == missionId)
                return def;
        return null;
    }

    public IReadOnlyList<DailyMissionState> GetMissions() => profile.Data.daily.missions;

    public bool TryClaimMission(string missionId)
    {
        var st = profile.Data.daily.missions.FirstOrDefault(m => m.missionId == missionId);
        if (st == null || !st.completed || st.claimed)
            return false;

        profile.AddGoldAndSave(st.rewardGold);
        st.claimed = true;
        profile.SaveProfile();
        OnDailyMissionsChanged?.Invoke();
        FireAttention();
        return true;
    }

    public DateTime GetNextResetTime()
    {
        var now = GetNow();
        return now.Date.AddDays(1);
    }

    // -------------------------
    // Report API (com out toast)
    // -------------------------
    public bool ReportWinLevel(int level, out string toastMsg, out int rewardGold)
    {
        toastMsg = null; rewardGold = 0;
        bool changed = false, completedNowAny = false;

        var list = profile.Data.daily.missions;
        if (list == null) return false;

        foreach (var st in list)
        {
            var def = FindDef(st.missionId);
            if (def == null || def.eventType != MissionEventType.WinLevel) continue;
            if (def.levelParam > 0 && def.levelParam != level) continue;
            if (st.completed) continue;

            int before = st.progress;
            st.progress = Mathf.Min(st.target, st.progress + 1);
            if (st.progress != before) changed = true;

            if (st.progress >= st.target && !st.completed)
            {
                st.completed = true;
                changed = true;
                completedNowAny = true;

                var text = BuildDescription(def, st);
                toastMsg = $"Missão concluída: {text}  +{st.rewardGold} moedas!";
                rewardGold = st.rewardGold;
            }
        }

        if (changed)
        {
            profile.SaveProfile();
            OnDailyMissionsChanged?.Invoke();
            FireAttention();
        }
        return completedNowAny;
    }

    public void ReportWinLevel(int level) // compat (sem quebrar chamadas antigas)
    {
        if (ReportWinLevel(level, out var toast, out _))
            ToastService.Show(toast);
    }

    public bool ReportPairMade(out string toastMsg, out int rewardGold, string cardTypeId = null)
    {
        return TryAddProgress(MissionEventType.MakePair, 1, out toastMsg, out rewardGold, cardTypeId);
    }

    public void ReportPairMade(string cardTypeId = null) // compat
    {
        if (ReportPairMade(out var toast, out _, cardTypeId))
            ToastService.Show(toast);
    }

    public bool ReportSwapAll(out string toastMsg, out int rewardGold)
    {
        return TryAddProgress(MissionEventType.SwapAll, 1, out toastMsg, out rewardGold, null);
    }
    public void ReportSwapAll() { if (ReportSwapAll(out var toast, out _)) ToastService.Show(toast); }

    public bool ReportSwapRandom(out string toastMsg, out int rewardGold)
    {
        return TryAddProgress(MissionEventType.SwapRandom, 1, out toastMsg, out rewardGold, null);
    }
    public void ReportSwapRandom() { if (ReportSwapRandom(out var toast, out _)) ToastService.Show(toast); }

    public bool ReportBuyCard(out string toastMsg, out int rewardGold)
    {
        return TryAddProgress(MissionEventType.BuyCard, 1, out toastMsg, out rewardGold, null);
    }
    public void ReportBuyCard() { if (ReportBuyCard(out var toast, out _)) ToastService.Show(toast); }

    public bool ReportRunFinished(out string toastMsg, out int rewardGold)
    {
        return TryAddProgress(MissionEventType.PlayRun, 1, out toastMsg, out rewardGold, null);
    }
    public void ReportRunFinished() { if (ReportRunFinished(out var toast, out _)) ToastService.Show(toast); }

    public bool ReportScoreDelta(int delta, out string toastMsg, out int rewardGold)
    {
        toastMsg = null; rewardGold = 0;
        if (delta <= 0) return false;
        return TryAddProgress(MissionEventType.ScorePoints, delta, out toastMsg, out rewardGold, null);
    }
    public void ReportScoreDelta(int delta)
    {
        if (ReportScoreDelta(delta, out var toast, out _))
            ToastService.Show(toast);
    }

    public bool ReportStars(int stars, out string toastMsg, out int rewardGold)
    {
        toastMsg = null; rewardGold = 0;
        if (stars <= 0) return false;
        return TryAddProgress(MissionEventType.StarsEarned, stars, out toastMsg, out rewardGold, null);
    }
    public void ReportStars(int stars)
    {
        if (ReportStars(stars, out var toast, out _))
            ToastService.Show(toast);
    }

    // -------------------------
    // Helpers / attention
    // -------------------------
    private DailyMissionSo FindDef(string missionId) =>
        missionPool.FirstOrDefault(m => m && m.id == missionId);

    public bool HasAnyClaimAvailable()
    {
        bool anyMission = profile.Data.daily.missions.Any(m => m.completed && !m.claimed);
        bool login = IsDailyLoginAvailable();
        return anyMission || login;
    }

    public bool HasMissionClaimAvailable()
    {
        var list = profile.Data?.daily?.missions;
        return list != null && list.Any(m => m.completed && !m.claimed);
    }

    public void FireAttention()
    {
        OnAttentionChanged?.Invoke(HasAnyClaimAvailable());
    }

    // -------------------------
    // Core progress (com toast out)
    // -------------------------
    private bool TryAddProgress(
        MissionEventType type,
        int amount,
        out string toastMsg,
        out int rewardGold,
        string param = null)
    {
        toastMsg = null;
        rewardGold = 0;

        var list = profile.Data?.daily?.missions;
        if (list == null) return false;

        bool changed = false;
        bool completedNowAny = false;

        foreach (var st in list)
        {
            var def = FindDef(st.missionId);
            if (def == null || def.eventType != type) continue;
            if (st.completed) continue;
            if (!string.IsNullOrEmpty(def.paramId) && def.paramId != param) continue;

            var before = st.progress;
            st.progress = Mathf.Min(st.target, st.progress + Mathf.Max(1, amount));
            if (st.progress != before) changed = true;

            if (st.progress >= st.target && !st.completed)
            {
                st.completed = true;
                changed = true;
                completedNowAny = true;

                var text = BuildDescription(def, st);
                toastMsg = $"Missão concluída: {text}  +{st.rewardGold} moedas!";
                rewardGold = st.rewardGold;
                // não faz claim aqui; apenas marca concluída e deixa pro jogador coletar
            }
        }

        if (changed)
        {
            profile.SaveProfile();
            OnDailyMissionsChanged?.Invoke();
            FireAttention();
        }

        return completedNowAny;
    }

    private string BuildDescription(DailyMissionSo def, DailyMissionState st)
    {
        // Usa o template do SO, senão cai no texto salvo no state
        string tpl = !string.IsNullOrEmpty(def?.descriptionTemplate) ? def.descriptionTemplate : st.description;
        if (string.IsNullOrEmpty(tpl)) tpl = st.missionId;

        // Substituições simples
        tpl = tpl.Replace("{level}", def.levelParam.ToString());
        tpl = tpl.Replace("{target}", st.target.ToString());

        // Formato {0} compatível (ex.: nível)
        try { tpl = string.Format(tpl, def.levelParam, st.target); } catch { /* ignore */ }

        return tpl;
    }
}
