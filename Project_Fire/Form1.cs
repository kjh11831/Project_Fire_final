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
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace Project_Fire
{
    public partial class Form1 : Form
    {
        private Form2 form2;
        private IFirebaseClient client;


        public Form1()
        {
            InitializeComponent();
            button1.Click += button1_Click_1;
        }

        IFeatureSupport irebaseConfig = new FirebaseConfig
        {
            AuthSecret = "",
            BasePath = ""
        };

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                client = new FireSharp.FirebaseClient(config);

                if (client == null)
                {
                    MessageBox.Show("Firebase 연결 실패!");
                    return;
                }

                // 데이터 가져오기
                FirebaseResponse response1 = await client.GetAsync("arduino1");
                FirebaseResponse response2 = await client.GetAsync("arduino2");

                if (response1.StatusCode == System.Net.HttpStatusCode.OK && response2.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    SensorData arduino1Data = response1.ResultAs<SensorData>();
                    SensorData arduino2Data = response2.ResultAs<SensorData>();
                }

                if (arduino1Data != null && arduino2Data != null)
                {
                    if (form2 == null || form2.IsDisposed)
                    {
                        form2 = new Form2();
                        form2.Show();
                    }

                    // Form2가 로드된 후에 데이터를 전달
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