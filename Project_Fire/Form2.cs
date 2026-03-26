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
        private IFirebaseClient client;
        private System.Timers.Timer updateTimer;
        private bool handleCreated = false;

        // 상수 설정 (한 곳에서 관리하면 수정이 편합니다)
        private const double THRESHOLD_CO2 = 800.0;
        private const double THRESHOLD_TEMP = 60.0;
        private const string FLAME_DETECTED = "1";
        private const double ERR_DIFF_CO2 = 300.0;
        private const double ERR_DIFF_TEMP = 20.0;

        public Form2()
        {
            InitializeComponent();
            this.Load += new EventHandler(Form2_Load);
            this.HandleCreated += (s, e) => handleCreated = true;

            // 타이머 초기화 (3초)
            updateTimer = new System.Timers.Timer(3000);
            updateTimer.Elapsed += UpdateTimer_Elapsed;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Form2_Load 호출됨");

            client = new FireSharp.FirebaseClient(new FirebaseConfig
            {
                AuthSecret = "6zja7ZvLbRxEuuTmvulUobmmelYMVnWHWz8g4fCp",
                BasePath = "https://projectfire-e50e7-default-rtdb.firebaseio.com"
            });

            if (client != null)
            {
                MessageBox.Show("Firebase 연결 성공!");
                updateTimer.Start();
            }

            else
            {
                MessageBox.Show("Firebase 연결 실패!");
            }
        }

        private async void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await FetchAndDisplayData();
        }

        // 1. 데이터 호출부
        private async Task FetchAndDisplayData()
        {
            try
            {
                FirebaseResponse r1 = await client.GetAsync("arduino1");
                FirebaseResponse r2 = await client.GetAsync("arduino2");

                SensorData data1 = r1.ResultAs<SensorData>();
                SensorData data2 = r2.ResultAs<SensorData>();

                if (data1 != null && data2 != null)
                {
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

        // 2. UI 업데이트 통합 관리 (중복 제거 핵심)
        private void UpdateUI(SensorData d1, SensorData d2)
        {
            if (!handleCreated) return;

            this.Invoke((MethodInvoker)delegate
            {
                // 화재 감지 여부 계산
                bool fire1 = CheckFireDetection(d1);
                bool fire2 = CheckFireDetection(d2);

                // 아파트 이미지 교체
                if (fire1 || fire2)
                {
                    apart.Image = Properties.Resources.화재아파트;
                }

                else
                {
                    apart.Image = Properties.Resources.아파트;
                }

                // 801호 이미지 교체
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

                // 802호 이미지 교체
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

                // 텍스트 정보 업데이트
                labelArduino1.Text = FormatSensorText("801호", d1);
                labelArduino2.Text = FormatSensorText("802호", d2);

                textBoxFire1.Text = fire1 ? "화재가 발생했습니다!" : "화재가 감지되지 않았습니다";
                textBoxFire2.Text = fire2 ? "화재가 발생했습니다!" : "화재가 감지되지 않았습니다";

                // 배경색 업데이트
                UpdateIndicatorColors(d1, d2);

                // 센서 오류 체크 (화재가 아닐 때만 수행)
                if (!fire1 && !fire2)
                {
                    CheckSensorErrors(d1, d2);
                }
            });
        }

        // --- 세부 로직 함수들 ---

        // 화재 판정 로직 (기본 2개 조건 충족 시 화재)
        private bool CheckFireDetection(SensorData data)
        {
            // 1. 불꽃이 감지되면 다른 조건 상관없이 즉시 화재로 판정
            if (data.Flame == FLAME_DETECTED) return true;

            // 2. 불꽃이 없더라도 CO2와 온도가 모두 높으면 화재로 판정 (보조 판정)
            bool highCO2 = double.TryParse(data.CO2, out double c) && c >= THRESHOLD_CO2;
            bool highTemp = double.TryParse(data.Temperature, out double t) && t >= THRESHOLD_TEMP;

            return highCO2 && highTemp;
        }

        // 센서 텍스트 포맷팅
        private string FormatSensorText(string roomName, SensorData data)
        {
            double.TryParse(data.CO2, out double c);
            double.TryParse(data.Temperature, out double t);
            string flameStatus = (data.Flame == "0" ? "불꽃 감지 없음" : "불꽃 감지!!");

            return $"{roomName}\nCO2: {c:F1} ppm\nTemp: {t:F1} °C\nFlame: {flameStatus}";
        }

        // 패널(표시등) 색상 업데이트
        private void UpdateIndicatorColors(SensorData d1, SensorData d2)
        {
            if (double.TryParse(d1.CO2, out double c1) && double.TryParse(d2.CO2, out double c2) &&
                double.TryParse(d1.Temperature, out double t1) && double.TryParse(d2.Temperature, out double t2))
            {
                C801.BackColor = c1 >= THRESHOLD_CO2 ? Color.Red : Color.Green;
                C802.BackColor = c2 >= THRESHOLD_CO2 ? Color.Red : Color.Green;
                T801.BackColor = t1 >= THRESHOLD_TEMP ? Color.Red : Color.Green;
                T802.BackColor = t2 >= THRESHOLD_TEMP ? Color.Red : Color.Green;
            }
        }

        // 센서 간 오차 체크 (오류 메시지)
        private void CheckSensorErrors(SensorData d1, SensorData d2)
        {
            bool isC1 = double.TryParse(d1.CO2, out double c1);
            bool isC2 = double.TryParse(d2.CO2, out double c2);
            bool isT1 = double.TryParse(d1.Temperature, out double t1);
            bool isT2 = double.TryParse(d2.Temperature, out double t2);

            string errorMsg = "";
            if (isC1 && isC2 && Math.Abs(c1 - c2) >= ERR_DIFF_CO2) errorMsg += "CO2 센서 오류\n";
            if (isT1 && isT2 && Math.Abs(t1 - t2) >= ERR_DIFF_TEMP) errorMsg += "온도 센서 오류\n";

            if (!string.IsNullOrEmpty(errorMsg))
            {
                MessageBox.Show(errorMsg);
            }
        }

        // 공통 메시지 박스 호출 (Invoke 처리)
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

        // 외부 호출용 (필요시 유지)
        internal void DisplaySensorData(SensorData d1, SensorData d2)
        {
            UpdateUI(d1, d2);
        }

        public class SensorData
        {
            public string CO2 { get; set; }
            public string Temperature { get; set; }
            public string Flame { get; set; }
        }
    }
}