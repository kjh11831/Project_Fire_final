using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Project_Fire.Form2;

namespace Project_Fire
{
    public partial class Form1 : Form
    {
        private Form2 form2; // 모니터링 화면(Form2) 객체 변수
        private IFirebaseClient client; // Firebase 연결을 위한 클라이언트 객체


        public Form1()
        {
            InitializeComponent();
            button1.Click += button1_Click; // 버튼 클릭 시 이벤트 연결
        }

        // [설정] Firebase 접속 정보 (비밀번호 및 데이터베이스 경로)
        IFirebaseConfig Config = new FirebaseConfig
        {
            AuthSecret = "6zja7ZvLbRxEuuTmvulUobmmelYMVnWHWz8g4fCp",
            BasePath = "https://projectfire-e50e7-default-rtdb.firebaseio.com/"
        };

        // [동작] 화재감지 버튼 클릭 시 실행
        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Firebase 서버에 연결 시도
                client = new FireSharp.FirebaseClient(Config);

                if (client == null)
                {
                    MessageBox.Show("Firebase 연결 실패!");
                    return;
                }

                // 2. 실시간 데이터베이스에서 아두이노 1, 2의 데이터를 비동기로 가져옴
                FirebaseResponse response1 = await client.GetAsync("arduino1");
                FirebaseResponse response2 = await client.GetAsync("arduino2");

                // 3. 응답이 성공(OK)인 경우 데이터 처리
                if (response1.StatusCode == System.Net.HttpStatusCode.OK && response2.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // JSON 데이터를 SensorData 클래스 형태로 변환
                    SensorData arduino1Data = response1.ResultAs<SensorData>();
                    SensorData arduino2Data = response2.ResultAs<SensorData>();

                    if (arduino1Data != null && arduino2Data != null)
                    {
                        // 4. 모니터링 창(Form2)이 없으면 새로 생성하여 띄움
                        if (form2 == null || form2.IsDisposed)
                        {
                            form2 = new Form2();
                            form2.Show();
                        }

                        // 5. 창이 로드된 후 첫 데이터를 전달하여 화면에 표시
                        form2.Load += (s, args) =>
                        {
                            form2.DisplaySensorData(arduino1Data, arduino2Data);
                        };
                    }

                    else
                    {
                        MessageBox.Show("데이터를 불러오는 데 실패했습니다.");
                    }
                }

                else
                {
                    MessageBox.Show("데이터를 불러오는 데 실패했습니다.");
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}");
            }
        }
    }
}