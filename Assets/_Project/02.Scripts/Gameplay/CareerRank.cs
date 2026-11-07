using System;

namespace OfficeWorker.Gameplay.Career
{
    /// <summary>
    /// 《내꿈은 정규직》 레퍼런스 12단계 직급 체계
    /// </summary>
    public enum CareerRank
    {
        Intern = 0,             // 인턴 (필요 경력 30, 승진 확률 50%, 실패 1회 시 즉시 해고)
        ContractWorker = 1,     // 계약직 (필요 경력 60, 승진 확률 40%, 실패 2회 연속 시 해고)
        FullTimeWorker = 2,     // 정규직 (필요 경력 120, 승진 확률 40%, 실패 3회 연속 시 해고)
        AssistantManager = 3,   // 대리 (필요 경력 300, 승진 확률 30%, 부하직원 1명 해금)
        Manager = 4,            // 과장 (필요 경력 600, 승진 확률 20%, 부하직원 2명)
        DeputyGeneralManager = 5,// 차장 (필요 경력 1200, 승진 확률 10%, 부하 갈구기 해금)
        GeneralManager = 6,     // 부장 (필요 경력 2400, 승진 확률 5%, 결재권)
        Director = 7,           // 이사 (필요 경력 5000, 승진 확률 3%, 임원실 이동)
        ManagingDirector = 8,   // 상무 (필요 경력 10000, 승진 확률 2%)
        SeniorManagingDirector=9,// 전무 (필요 경력 20000, 승진 확률 1%)
        VicePresident = 10,     // 부사장 (필요 경력 50000, 승진 확률 0.5%)
        President = 11          // 사장 (최종 목표, 게임 클리어 엔딩)
    }

    /// <summary>
    /// 직급별 기본 밸런스 설정 정보
    /// </summary>
    [Serializable]
    public struct RankConfig
    {
        public CareerRank Rank;
        public string TitleKorean;
        public int RequiredExp;
        public float BasePromotionChance; // 0.0f ~ 1.0f
        public int MaxFailAllowance;      // 연속 실패 허용 횟수
        public long DailyBaseSalary;      // 일일 기본급 (퇴직금 계산용)
        public int SubordinateCount;      // 배치되는 부하직원 수

        public RankConfig(CareerRank rank, string titleKr, int reqExp, float chance, int maxFail, long salary, int subordinates)
        {
            Rank = rank;
            TitleKorean = titleKr;
            RequiredExp = reqExp;
            BasePromotionChance = chance;
            MaxFailAllowance = maxFail;
            DailyBaseSalary = salary;
            SubordinateCount = subordinates;
        }
    }

    public static class CareerRankExtensions
    {
        public static RankConfig GetConfig(this CareerRank rank)
        {
            return rank switch
            {
                CareerRank.Intern => new RankConfig(rank, "인턴", 30, 0.50f, 1, 0, 0),
                CareerRank.ContractWorker => new RankConfig(rank, "계약직", 60, 0.40f, 2, 50000, 0),
                CareerRank.FullTimeWorker => new RankConfig(rank, "정규직", 120, 0.40f, 3, 100000, 0),
                CareerRank.AssistantManager => new RankConfig(rank, "대리", 300, 0.30f, 3, 150000, 1),
                CareerRank.Manager => new RankConfig(rank, "과장", 600, 0.20f, 3, 220000, 2),
                CareerRank.DeputyGeneralManager => new RankConfig(rank, "차장", 1200, 0.10f, 3, 300000, 2),
                CareerRank.GeneralManager => new RankConfig(rank, "부장", 2400, 0.05f, 3, 400000, 3),
                CareerRank.Director => new RankConfig(rank, "이사", 5000, 0.03f, 3, 600000, 3),
                CareerRank.ManagingDirector => new RankConfig(rank, "상무", 10000, 0.02f, 3, 800000, 4),
                CareerRank.SeniorManagingDirector => new RankConfig(rank, "전무", 20000, 0.01f, 3, 1200000, 4),
                CareerRank.VicePresident => new RankConfig(rank, "부사장", 50000, 0.005f, 3, 2000000, 5),
                CareerRank.President => new RankConfig(rank, "사장", 0, 1.0f, 0, 5000000, 10),
                _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null)
            };
        }
    }
}
