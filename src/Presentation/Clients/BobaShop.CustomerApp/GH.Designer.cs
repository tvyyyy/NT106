namespace BobaShop.CustomerApp
{
    partial class GH
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer1 = new SplitContainer();
            lblMainTitle = new Label();
            flpCartItems = new FlowLayoutPanel();
            pnlSummary = new Panel();
            label1 = new Label();
            label2 = new Label();
            rdoDineIn = new RadioButton();
            rdoTakeaway = new RadioButton();
            label3 = new Label();
            txtVoucher = new TextBox();
            btnApplyVoucher = new Button();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            rdoCash = new RadioButton();
            btnCheckout = new Button();
            rdoPayOS = new RadioButton();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            pnlSummary.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(flpCartItems);
            splitContainer1.Panel1.Controls.Add(lblMainTitle);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(pnlSummary);
            splitContainer1.Size = new Size(938, 564);
            splitContainer1.SplitterDistance = 580;
            splitContainer1.TabIndex = 0;
            // 
            // lblMainTitle
            // 
            lblMainTitle.AutoSize = true;
            lblMainTitle.Font = new Font("Times New Roman", 16F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblMainTitle.ForeColor = Color.Green;
            lblMainTitle.Location = new Point(25, 20);
            lblMainTitle.Name = "lblMainTitle";
            lblMainTitle.Size = new Size(256, 36);
            lblMainTitle.TabIndex = 0;
            lblMainTitle.Text = "Giỏ hàng của bạn";
            // 
            // flpCartItems
            // 
            flpCartItems.AutoScroll = true;
            flpCartItems.BackColor = Color.Transparent;
            flpCartItems.FlowDirection = FlowDirection.TopDown;
            flpCartItems.Location = new Point(25, 60);
            flpCartItems.Name = "flpCartItems";
            flpCartItems.Size = new Size(545, 490);
            flpCartItems.TabIndex = 1;
            flpCartItems.WrapContents = false;
            // 
            // pnlSummary
            // 
            pnlSummary.BackColor = Color.White;
            pnlSummary.Controls.Add(rdoPayOS);
            pnlSummary.Controls.Add(btnCheckout);
            pnlSummary.Controls.Add(rdoCash);
            pnlSummary.Controls.Add(label6);
            pnlSummary.Controls.Add(label5);
            pnlSummary.Controls.Add(label4);
            pnlSummary.Controls.Add(btnApplyVoucher);
            pnlSummary.Controls.Add(txtVoucher);
            pnlSummary.Controls.Add(label3);
            pnlSummary.Controls.Add(rdoTakeaway);
            pnlSummary.Controls.Add(rdoDineIn);
            pnlSummary.Controls.Add(label2);
            pnlSummary.Controls.Add(label1);
            pnlSummary.Location = new Point(-8, 15);
            pnlSummary.Name = "pnlSummary";
            pnlSummary.Size = new Size(350, 530);
            pnlSummary.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Georgia", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.FromArgb(38, 99, 59);
            label1.Location = new Point(11, 8);
            label1.Name = "label1";
            label1.Size = new Size(314, 29);
            label1.TabIndex = 0;
            label1.Text = "THÔNG TIN ĐƠN HÀNG";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Times New Roman", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(15, 50);
            label2.Name = "label2";
            label2.Size = new Size(175, 20);
            label2.TabIndex = 1;
            label2.Text = "Hình thức nhận hàng";
            // 
            // rdoDineIn
            // 
            rdoDineIn.AutoSize = true;
            rdoDineIn.Location = new Point(15, 75);
            rdoDineIn.Name = "rdoDineIn";
            rdoDineIn.Size = new Size(93, 29);
            rdoDineIn.TabIndex = 2;
            rdoDineIn.Text = "Tại chỗ";
            rdoDineIn.UseVisualStyleBackColor = true;
            // 
            // rdoTakeaway
            // 
            rdoTakeaway.AutoSize = true;
            rdoTakeaway.Location = new Point(140, 75);
            rdoTakeaway.Name = "rdoTakeaway";
            rdoTakeaway.Size = new Size(106, 29);
            rdoTakeaway.TabIndex = 3;
            rdoTakeaway.Text = "Mang về";
            rdoTakeaway.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Georgia", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(15, 120);
            label3.Name = "label3";
            label3.Size = new Size(207, 21);
            label3.TabIndex = 4;
            label3.Text = "Voucher/Mã giảm giá";
            // 
            // txtVoucher
            // 
            txtVoucher.Location = new Point(15, 153);
            txtVoucher.Name = "txtVoucher";
            txtVoucher.PlaceholderText = "Nhập mã voucher";
            txtVoucher.Size = new Size(206, 31);
            txtVoucher.TabIndex = 5;
            // 
            // btnApplyVoucher
            // 
            btnApplyVoucher.BackColor = Color.FromArgb(38, 99, 59);
            btnApplyVoucher.FlatStyle = FlatStyle.Flat;
            btnApplyVoucher.ForeColor = Color.White;
            btnApplyVoucher.Location = new Point(228, 149);
            btnApplyVoucher.Name = "btnApplyVoucher";
            btnApplyVoucher.Size = new Size(97, 38);
            btnApplyVoucher.TabIndex = 6;
            btnApplyVoucher.Text = "Áp dụng";
            btnApplyVoucher.UseVisualStyleBackColor = false;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(14, 210);
            label4.Name = "label4";
            label4.Size = new Size(81, 25);
            label4.TabIndex = 7;
            label4.Text = "Tạm tính";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(14, 235);
            label5.Name = "label5";
            label5.Size = new Size(82, 25);
            label5.TabIndex = 8;
            label5.Text = "Giảm giá";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(14, 275);
            label6.Name = "label6";
            label6.Size = new Size(98, 25);
            label6.TabIndex = 9;
            label6.Text = "Tổng cộng";
            // 
            // rdoCash
            // 
            rdoCash.AutoSize = true;
            rdoCash.Checked = true;
            rdoCash.Location = new Point(15, 345);
            rdoCash.Name = "rdoCash";
            rdoCash.Size = new Size(226, 29);
            rdoCash.TabIndex = 10;
            rdoCash.TabStop = true;
            rdoCash.Text = "Tiền mặt tại quầy (Cash)";
            rdoCash.UseVisualStyleBackColor = true;
            // 
            // btnCheckout
            // 
            btnCheckout.BackColor = Color.FromArgb(38, 99, 59);
            btnCheckout.FlatStyle = FlatStyle.Flat;
            btnCheckout.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCheckout.ForeColor = Color.White;
            btnCheckout.Location = new Point(15, 440);
            btnCheckout.Name = "btnCheckout";
            btnCheckout.Size = new Size(275, 45);
            btnCheckout.TabIndex = 11;
            btnCheckout.Text = "XÁC NHẬN ĐẶT HÀNG";
            btnCheckout.UseVisualStyleBackColor = false;
            // 
            // rdoPayOS
            // 
            rdoPayOS.AutoSize = true;
            rdoPayOS.Location = new Point(15, 375);
            rdoPayOS.Name = "rdoPayOS";
            rdoPayOS.Size = new Size(246, 29);
            rdoPayOS.TabIndex = 13;
            rdoPayOS.TabStop = true;
            rdoPayOS.Text = "Chuyển khoản QR (PayOS)";
            rdoPayOS.UseVisualStyleBackColor = true;
            // 
            // GH
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Cornsilk;
            ClientSize = new Size(938, 564);
            Controls.Add(splitContainer1);
            Name = "GH";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Giỏ hàng - Boba milk tea";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            pnlSummary.ResumeLayout(false);
            pnlSummary.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private Label lblMainTitle;
        private FlowLayoutPanel flpCartItems;
        private Panel pnlSummary;
        private Label label1;
        private RadioButton rdoDineIn;
        private Label label2;
        private Button btnApplyVoucher;
        private TextBox txtVoucher;
        private Label label3;
        private RadioButton rdoTakeaway;
        private Label label6;
        private Label label5;
        private Label label4;
        private RadioButton rdoPayOS;
        private Button btnCheckout;
        private RadioButton rdoCash;
    }
}