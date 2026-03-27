using System;
using System.Windows.Forms;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System.Timers;
using System.Drawing;
using System.Threading.Tasks;

namespace Project_Fire
{
    public partial class Form2 : Form
    {
        // --- [전역 변수 및 객체 선언] ---
        private IFirebaseClient client; // Firebase 서버와 통신하기 위한 클라이언트 객체
        private System.Timers.Timer updateTimer; // 일정 시간마다 데이터를 갱신할 타이머
        private bool handleCreated = false; // 윈도우 폼이 완전히 생성되었는지 확인하는 플래그

        // --- [시스템 판단 기준 상수] ---
        private const double THRESHOLD_CO2 = 800.0; // CO2 농도 위험 기준치 (800ppm)
        private const double THRESHOLD_TEMP = 60.0; // 온도 위험 기준치 (60도)
        private const string FLAME_DETECTED = "1"; // 불꽃 감지 시 아두이노에서 보내는 신호 값
        private const double ERR_DIFF_CO2 = 300.0; // 두 방 사이의 CO2 오차 허용 범위 (센서 오류 판단용)
        private const double ERR_DIFF_TEMP = 20.0; // 두 방 사이의 온도 오차 허용 범위 (센서 오류 판단용)

        // --- [생성자: 프로그램 시작 시 초기 설정] ---
        public Form2()
        {
            InitializeComponent(); // 화면 UI 요소(버튼, 라벨 등) 초기화
            this.Load += new EventHandler(Form2_Load); // 폼 로드 시 실행될 이벤트 연결
            this.HandleCreated += (s, e) => handleCreated = true; // 폼 핸들 생성 완료 체크

            // 타이머 초기설정: 3000ms(3초) 주기로 실행되도록 설정
            updateTimer = new System.Timers.Timer(3000);
            updateTimer.Elapsed += UpdateTimer_Elapsed; // 3초마다 UpdateTimer_Elapsed 함수 실행
        }

        // --- [Firebase 연결 설정] ---
        private void Form2_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Form2_Load 호출됨"); // 디버깅용 메시지

            // Firebase 접속 정보 설정 (비밀번호 및 DB 주소)
            client = new FireSharp.FirebaseClient(new FirebaseConfig
            {
                AuthSecret = "6zja7ZvLbRxEuuTmvulUobmmelYMVnWHWz8g4fCp",
                BasePath = "https://projectfire-e50e7-default-rtdb.firebaseio.com"
            });

            if (client != null) // 연결 성공 시
            {
                MessageBox.Show("Firebase 연결 성공!");
                updateTimer.Start(); // 3초 주기 타이머 시작
            }

            else // 연결 실패 시
            {
                MessageBox.Show("Firebase 연결 실패!");
            }
        }

        // --- [데이터 갱신 루프] ---
        private async void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // 타이머가 울릴 때마다 비동기로 데이터를 가져와 표시함
            await FetchAndDisplayData();
        }

        // --- [1. 데이터 호출 및 파싱 부] ---
        private async Task FetchAndDisplayData()
        {
            try
            {
                // Firebase에서 arduino1, arduino2 경로의 데이터를 각각 가져옴
                FirebaseResponse r1 = await client.GetAsync("arduino1");
                FirebaseResponse r2 = await client.GetAsync("arduino2");

                // 수신된 JSON 데이터를 SensorData 클래스 구조로 변환
                SensorData data1 = r1.ResultAs<SensorData>();
                SensorData data2 = r2.ResultAs<SensorData>();

                if (data1 != null && data2 != null)
                {
                    // 데이터가 정상적으로 있으면 UI 업데이트 함수 호출
                    UpdateUI(data1, data2);
                }

                else
                {
                    ShowMessage("데이터를 불러오는 데 실패했습니다.");
                }
            }

            catch (Exception ex)
            {
                ShowMessage($"오류 발생: {ex.Message}");
            }
        }

        // --- [2. UI 업데이트 통합 제어 (시각화 핵심)] ---
        private void UpdateUI(SensorData d1, SensorData d2)
        {
            if (!handleCreated) return; // 폼이 아직 준비 안 됐으면 중단

            // 타이머 스레드에서 UI 스레드로 접근하기 위한 Invoke 사용
            this.Invoke((MethodInvoker)delegate
            {
                // 화재 여부를 알고리즘에 따라 판정 (true/false)
                bool fire1 = CheckFireDetection(d1); // 801호 판정
                bool fire2 = CheckFireDetection(d2); // 802호 판정

                // [아파트 외관 업데이트] 둘 중 한 곳이라도 불나면 화재 이미지로 교체
                if (fire1 || fire2)
                {
                    apart.Image = Properties.Resources.화재아파트;
                }

                else
                {
                    apart.Image = Properties.Resources.아파트;
                }

                // [801호 상태 표시] 화재 시 불꽃 이미지 노출, 평상시 녹색 배경
                if (fire1)
                {
                    F801.BackColor = Color.Transparent;
                    F801.Image = Properties.Resources.불꽃;
                }

                else
                {
                    F801.Image = null;
                    F801.BackColor = Color.Green;
                }

                // [802호 상태 표시]
                if (fire2)
                {
                    F802.BackColor = Color.Transparent;
                    F802.Image = Properties.Resources.불꽃;
                }

                else
                {
                    F802.Image = null;
                    F802.BackColor = Color.Green;
                }

                // [텍스트 정보 업데이트] 수치를 라벨과 텍스트박스에 출력
                labelArduino1.Text = FormatSensorText("801호", d1);
                labelArduino2.Text = FormatSensorText("802호", d2);

                textBoxFire1.Text = fire1 ? "화재가 발생했습니다!" : "화재가 감지되지 않았습니다";
                textBoxFire2.Text = fire2 ? "화재가 발생했습니다!" : "화재가 감지되지 않았습니다";

                // [개별 센서 인디케이터 색상] (온도/CO2 개별 위험 체크)
                UpdateIndicatorColors(d1, d2);

                // [센서 고장 진단] 화재 상황이 아닐 때만 수치 비교를 통해 고장 여부 확인
                if (!fire1 && !fire2)
                {
                    CheckSensorErrors(d1, d2);
                }
            });
        }

        // --- [3. 세부 로직 함수들] ---

        // 화재 확정 판정 알고리즘
        private bool CheckFireDetection(SensorData data)
        {
            // 1. 불꽃 센서값이 '1'이면 다른 조건 무시하고 즉시 화재로 간주
            if (data.Flame == FLAME_DETECTED) return true;

            // 2. 불꽃은 없지만 CO2와 온도가 동시에 임계치를 넘으면 화재로 간주 (복합 판정)
            bool highCO2 = double.TryParse(data.CO2, out double c) && c >= THRESHOLD_CO2;
            bool highTemp = double.TryParse(data.Temperature, out double t) && t >= THRESHOLD_TEMP;

            return highCO2 && highTemp; // 둘 다 높을 때만 true 반환
        }

        // 화면에 보여줄 센서 데이터 문자열 가공
        private string FormatSensorText(string roomName, SensorData data)
        {
            double.TryParse(data.CO2, out double c);
            double.TryParse(data.Temperature, out double t);
            string flameStatus = (data.Flame == "0" ? "불꽃 감지 없음" : "불꽃 감지!!");

            return $"{roomName}\nCO2: {c:F1} ppm\nTemp: {t:F1} °C\nFlame: {flameStatus}";
        }

        // 각 수치별 경고등 색상 업데이트
        private void UpdateIndicatorColors(SensorData d1, SensorData d2)
        {
            if (double.TryParse(d1.CO2, out double c1) && double.TryParse(d2.CO2, out double c2) &&
                double.TryParse(d1.Temperature, out double t1) && double.TryParse(d2.Temperature, out double t2))
            {
                // 기준치 이상이면 빨강, 아니면 초록
                C801.BackColor = c1 >= THRESHOLD_CO2 ? Color.Red : Color.Green;
                C802.BackColor = c2 >= THRESHOLD_CO2 ? Color.Red : Color.Green;
                T801.BackColor = t1 >= THRESHOLD_TEMP ? Color.Red : Color.Green;
                T802.BackColor = t2 >= THRESHOLD_TEMP ? Color.Red : Color.Green;
            }
        }

        // 센서 간 수치 차이가 너무 크면 고장으로 간주하고 경고창 출력
        private void CheckSensorErrors(SensorData d1, SensorData d2)
        {
            bool isC1 = double.TryParse(d1.CO2, out double c1);
            bool isC2 = double.TryParse(d2.CO2, out double c2);
            bool isT1 = double.TryParse(d1.Temperature, out double t1);
            bool isT2 = double.TryParse(d2.Temperature, out double t2);

            string errorMsg = "";
            // Math.Abs: 절댓값 계산 (두 수의 차이가 기준값보다 크면 에러)
            if (isC1 && isC2 && Math.Abs(c1 - c2) >= ERR_DIFF_CO2) errorMsg += "CO2 센서 오류\n";
            if (isT1 && isT2 && Math.Abs(t1 - t2) >= ERR_DIFF_TEMP) errorMsg += "온도 센서 오류\n";

            if (!string.IsNullOrEmpty(errorMsg))
            {
                MessageBox.Show(errorMsg + "(점검이 필요합니다)");
            }
        }

        // 안전한 메시지 박스 출력 (스레드 충돌 방지용)
        private void ShowMessage(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => MessageBox.Show(message)));
            }

            else
            {
                MessageBox.Show(message);
            }
        }

        // 외부(Form1)에서 데이터를 수동으로 밀어넣을 때 사용
        internal void DisplaySensorData(SensorData d1, SensorData d2)
        {
            UpdateUI(d1, d2);
        }

        // [데이터 모델] 아두이노에서 넘어온 값을 담는 구조
        public class SensorData
        {
            public string CO2 { get; set; } // 이산화탄소 농도
            public string Temperature { get; set; } // 온도 값
            public string Flame { get; set; } // 불꽃 감지 여부 (0 또는 1)
        }
    }
}