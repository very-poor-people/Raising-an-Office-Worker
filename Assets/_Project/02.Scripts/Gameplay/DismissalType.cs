using System;

namespace OfficeWorker.Gameplay.Dismissal
{
    /// <summary>
    /// 《내꿈은 정규직》 스타일 25종 부조리한 해고 사유 고유 ID
    /// </summary>
    public enum DismissalId
    {
        D01_InternFail = 1,         // 인턴 승진 심사 탈락 (1회 실패 즉시 방출)
        D02_ContractFail,           // 계약직 2회 연속 승진 실패
        D03_FullTimeFail,           // 정규직 이상 3회 연속 승진 실패
        D04_BurnOut,                // 과로로 체력 0 도달 (자진 퇴사)
        D05_DeskOverflow,           // 책상에 서류가 5개 이상 적체 (압사)
        D06_BossIgnored,            // 상사의 업무 호출 말풍선 30초 이상 방치
        D07_ScreenshotLeak,         // 화면 캡처 3회 클릭 (사내 기밀 유출)
        D08_EarlyLeave,             // 근무 시간 중 뒤로가기/종료 시도 (감히 퇴근을 해?!)
        D09_Absentee,               // 24시간 동안 미접속 (내 자리가 사라졌다...OTZ)
        D10_PeanutDisaster,         // 돌발 이벤트에서 땅콩을 봉지째 드림
        D11_VitaminCrime,           // 상사 심부름 중 비타민 박스를 열어봄 (뇌물 적발)
        D12_NitrogenSnack,          // 간식 선택에서 감자칩 뜯다 질소 폭풍으로 사무실 초토화
        D13_SubordinateStrike,      // 부하직원에게 일만 몰아주다 단체 파업 발생
        D14_SubordinateNeglect,     // 부하직원에게 일을 전혀 주지 않아 관리 부실 문책
        D15_TwoJobBusted,           // 근무 중 알바(광고 시청) 적발
        D16_TooEnthusiastic,        // 인턴 첫날 초고속 탭으로 선배들 눈 밖에 남
        D17_OfficeDinnerRefused,    // 회식 3회 연속 거절로 사내 화합 저해
        D18_PoliticsFail,           // 사내 줄서기 이벤트 줄타기 실패
        D19_ElevatorSilence,        // 사장과 단둘이 탄 엘리베이터에서 어색한 침묵
        D20_ColdCoffee,             // 상사에게 미지근한 캔커피 제출
        D21_MessengerGossip,        // 단톡방에 상사 뒷담화 잘못 전송
        D22_SingleHandCard,         // 거래처 미팅에서 명함을 한 손으로 건넴
        D23_SqueakyChair,           // 100회 초고속 탭으로 인한 소음 공해
        D24_NightSoloLeave,         // 야근 중 불 꺼진 사무실에서 혼자 몰래 퇴근
        D25_RestroomStrike          // 화장실 휴식 시간 초과로 근무지 이탈
    }

    [Serializable]
    public class DismissalInfo
    {
        public DismissalId Id;
        public string Title;
        public string SatireDescription;
        public bool IsUnlocked;

        public DismissalInfo(DismissalId id, string title, string satireDesc)
        {
            Id = id;
            Title = title;
            SatireDescription = satireDesc;
            IsUnlocked = false;
        }
    }
}
