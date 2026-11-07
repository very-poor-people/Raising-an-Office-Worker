using System;
using System.Collections.Generic;
using System.Linq;
using OfficeWorker.Gameplay.Career;

namespace OfficeWorker.Gameplay.Dismissal
{
    public class DismissalController
    {
        private readonly Dictionary<DismissalId, DismissalInfo> _catalog = new();

        public IReadOnlyDictionary<DismissalId, DismissalInfo> Catalog => _catalog;

        public event Action<DismissalInfo, long> OnDismissalOccurred; // info, severancePay

        public DismissalController()
        {
            InitializeCatalog();
        }

        private void InitializeCatalog()
        {
            var allIds = (DismissalId[])Enum.GetValues(typeof(DismissalId));
            foreach (var id in allIds)
            {
                _catalog[id] = new DismissalInfo(id, GetDefaultTitle(id), GetDefaultDesc(id));
            }
        }

        public void TriggerDismissal(DismissalId id, CareerRank currentRank, int daysWorked)
        {
            if (!_catalog.TryGetValue(id, out var info)) return;

            info.IsUnlocked = true;
            float collectionRate = GetCollectionRate();
            long severance = Core.GameDefines.CalculateSeverancePay(currentRank, daysWorked, collectionRate);

            OnDismissalOccurred?.Invoke(info, severance);
        }

        public float GetCollectionRate()
        {
            if (_catalog.Count == 0) return 0f;
            int unlockedCount = _catalog.Values.Count(x => x.IsUnlocked);
            return (float)unlockedCount / _catalog.Count;
        }

        public void Unlock(DismissalId id)
        {
            if (_catalog.TryGetValue(id, out var info))
            {
                info.IsUnlocked = true;
            }
        }

        private static string GetDefaultTitle(DismissalId id) => id switch
        {
            DismissalId.D01_InternFail => "인턴의 비애",
            DismissalId.D02_ContractFail => "계약 만료",
            DismissalId.D03_FullTimeFail => "권고사직",
            DismissalId.D04_BurnOut => "과로사 직전 퇴사",
            DismissalId.D05_DeskOverflow => "서류 더미 압사",
            DismissalId.D06_BossIgnored => "상사 부름 무시",
            DismissalId.D07_ScreenshotLeak => "회사 기밀 유출",
            DismissalId.D08_EarlyLeave => "감히 퇴근을 해?!",
            DismissalId.D09_Absentee => "내 자리가 사라졌다...OTZ",
            DismissalId.D10_PeanutDisaster => "봉지째 가져간 땅콩",
            DismissalId.D11_VitaminCrime => "비타민 박스의 비밀",
            DismissalId.D12_NitrogenSnack => "질소 과자의 최후",
            DismissalId.D13_SubordinateStrike => "부하직원 전원 파업",
            DismissalId.D14_SubordinateNeglect => "부하직원 태만 방치",
            DismissalId.D15_TwoJobBusted => "투잡 뛰다 걸렸다",
            DismissalId.D16_TooEnthusiastic => "신입의 과도한 열정",
            DismissalId.D17_OfficeDinnerRefused => "회식 3연속 거절",
            DismissalId.D18_PoliticsFail => "정치질 줄타기 실패",
            DismissalId.D19_ElevatorSilence => "엘리베이터의 어색한 침묵",
            DismissalId.D20_ColdCoffee => "미지근한 캔커피",
            DismissalId.D21_MessengerGossip => "단톡방 뒷담화 대참사",
            DismissalId.D22_SingleHandCard => "한 손으로 건넨 명함",
            DismissalId.D23_SqueakyChair => "의자 소음 공해",
            DismissalId.D24_NightSoloLeave => "야근 중 나홀로 퇴근",
            DismissalId.D25_RestroomStrike => "화장실 장기 농성",
            _ => "의문의 해고"
        };

        private static string GetDefaultDesc(DismissalId id) => id switch
        {
            DismissalId.D01_InternFail => "우린 실전형 인재를 원합니다. 수습 첫 탈락으로 짐을 쌉니다.",
            DismissalId.D02_ContractFail => "다음 기회에 정규직 전환을 노려보세요. 2연속 탈락입니다.",
            DismissalId.D03_FullTimeFail => "책상 위에 권고사직 통보서가 놓여 있습니다.",
            DismissalId.D04_BurnOut => "체력이 바닥나 응급실로 실려가며 자진 퇴사했습니다.",
            DismissalId.D05_DeskOverflow => "쌓아둔 서류 더미가 무너지며 압사당했습니다.",
            DismissalId.D06_BossIgnored => "상사가 부르는데 멍때리다 즉시 쫓겨났습니다.",
            _ => "회사 규정 위반으로 해고되었습니다."
        };
    }
}
