namespace Project_Fire
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.Fire_detector = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.Fire_detector)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.AutoSize = true;
            this.button1.BackColor = System.Drawing.Color.White;
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.CornflowerBlue;
            this.button1.FlatAppearance.BorderSize = 2;
            this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.button1.Location = new System.Drawing.Point(139, 329);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(106, 42);
            this.button1.TabIndex = 0;
            this.button1.Text = "화재감지";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Fire_detector
            // 
            this.Fire_detector.Image = global::Project_Fire.Properties.Resources.화재감지기;
            this.Fire_detector.Location = new System.Drawing.Point(71, 66);
            this.Fire_detector.Name = "Fire_detector";
            this.Fire_detector.Size = new System.Drawing.Size(246, 234);
            this.Fire_detector.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Fire_detector.TabIndex = 1;
            this.Fire_detector.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Info;
            this.ClientSize = new System.Drawing.Size(393, 436);
            this.Controls.Add(this.Fire_detector);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "화재경보기";
            ((System.ComponentModel.ISupportInitialize)(this.Fire_detector)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox Fire_detector;
    }
}

