namespace BobaShop.CustomerApp
{
    partial class CartItemControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            chkSelect = new CheckBox();
            picProduct = new PictureBox();
            lblProductName = new Label();
            lblDetails = new Label();
            lblToppings = new Label();
            txtNote = new TextBox();
            btnDelete = new Label();
            lblUnitPrice = new Label();
            lblTotalPrice = new Label();
            pnlQuantity = new Panel();
            btnMinus = new Button();
            lblQuantity = new Label();
            btnPlus = new Button();
            ((System.ComponentModel.ISupportInitialize)picProduct).BeginInit();
            pnlQuantity.SuspendLayout();
            SuspendLayout();
            // 
            // chkSelect
            // 
            chkSelect.AutoSize = true;
            chkSelect.Checked = true;
            chkSelect.CheckState = CheckState.Checked;
            chkSelect.Location = new Point(8, 17);
            chkSelect.Name = "chkSelect";
            chkSelect.Size = new Size(22, 21);
            chkSelect.TabIndex = 0;
            chkSelect.UseVisualStyleBackColor = true;
            chkSelect.CheckedChanged += chkSelect_CheckedChanged;
            // 
            // picProduct
            // 
            picProduct.BorderStyle = BorderStyle.FixedSingle;
            picProduct.Image = Properties.Resources.Cach_pha_tra_sua_hoa_nhai_ngon_la_mieng;
            picProduct.Location = new Point(36, 10);
            picProduct.Name = "picProduct";
            picProduct.Size = new Size(100, 113);
            picProduct.SizeMode = PictureBoxSizeMode.StretchImage;
            picProduct.TabIndex = 1;
            picProduct.TabStop = false;
            picProduct.Click += picProduct_Click;
            // 
            // lblProductName
            // 
            lblProductName.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblProductName.ForeColor = Color.FromArgb(30, 30, 30);
            lblProductName.Location = new Point(145, 10);
            lblProductName.Name = "lblProductName";
            lblProductName.Size = new Size(240, 22);
            lblProductName.TabIndex = 2;
            lblProductName.Text = "Trà Sữa Nhài";
            // 
            // lblDetails
            // 
            lblDetails.Font = new Font("Segoe UI", 8.5F);
            lblDetails.ForeColor = Color.DimGray;
            lblDetails.Location = new Point(145, 34);
            lblDetails.Name = "lblDetails";
            lblDetails.Size = new Size(260, 18);
            lblDetails.TabIndex = 3;
            lblDetails.Text = "Size: L | Đường: 70% | Đá: 50%";
            // 
            // lblToppings
            // 
            lblToppings.Font = new Font("Segoe UI", 8.5F);
            lblToppings.ForeColor = Color.DimGray;
            lblToppings.Location = new Point(145, 52);
            lblToppings.Name = "lblToppings";
            lblToppings.Size = new Size(260, 18);
            lblToppings.TabIndex = 4;
            lblToppings.Text = "Topping: Trân châu đen (+5k)";
            // 
            // txtNote
            // 
            txtNote.BorderStyle = BorderStyle.FixedSingle;
            txtNote.Location = new Point(145, 76);
            txtNote.Name = "txtNote";
            txtNote.PlaceholderText = "✏️ Ghi chú món này...";
            txtNote.Size = new Size(210, 31);
            txtNote.TabIndex = 5;
            txtNote.TextChanged += txtNote_TextChanged;
            // 
            // btnDelete
            // 
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDelete.ForeColor = Color.FromArgb(184, 42, 42);
            btnDelete.Location = new Point(515, 8);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(22, 22);
            btnDelete.TabIndex = 6;
            btnDelete.Text = "✕";
            btnDelete.Click += btnDelete_Click;
            // 
            // lblUnitPrice
            // 
            lblUnitPrice.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblUnitPrice.Location = new Point(415, 30);
            lblUnitPrice.Name = "lblUnitPrice";
            lblUnitPrice.Size = new Size(100, 20);
            lblUnitPrice.TabIndex = 7;
            lblUnitPrice.Text = "49.000đ";
            lblUnitPrice.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTotalPrice
            // 
            lblTotalPrice.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotalPrice.ForeColor = Color.FromArgb(184, 42, 42);
            lblTotalPrice.Location = new Point(415, 90);
            lblTotalPrice.Name = "lblTotalPrice";
            lblTotalPrice.Size = new Size(100, 36);
            lblTotalPrice.TabIndex = 8;
            lblTotalPrice.Text = "49.000đ";
            lblTotalPrice.TextAlign = ContentAlignment.MiddleRight;
            // 
            // pnlQuantity
            // 
            pnlQuantity.BorderStyle = BorderStyle.FixedSingle;
            pnlQuantity.Controls.Add(btnMinus);
            pnlQuantity.Controls.Add(lblQuantity);
            pnlQuantity.Controls.Add(btnPlus);
            pnlQuantity.Location = new Point(420, 58);
            pnlQuantity.Name = "pnlQuantity";
            pnlQuantity.Size = new Size(95, 28);
            pnlQuantity.TabIndex = 9;
            // 
            // btnMinus
            // 
            btnMinus.FlatAppearance.BorderSize = 0;
            btnMinus.FlatStyle = FlatStyle.Flat;
            btnMinus.Location = new Point(3, -3);
            btnMinus.Name = "btnMinus";
            btnMinus.Size = new Size(28, 26);
            btnMinus.TabIndex = 0;
            btnMinus.Text = "-";
            btnMinus.UseVisualStyleBackColor = true;
            btnMinus.Click += btnMinus_Click;
            // 
            // lblQuantity
            // 
            lblQuantity.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblQuantity.Location = new Point(28, 0);
            lblQuantity.Name = "lblQuantity";
            lblQuantity.Size = new Size(37, 26);
            lblQuantity.TabIndex = 1;
            lblQuantity.Text = "01";
            lblQuantity.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnPlus
            // 
            btnPlus.FlatAppearance.BorderSize = 0;
            btnPlus.FlatStyle = FlatStyle.Flat;
            btnPlus.Location = new Point(62, -3);
            btnPlus.Name = "btnPlus";
            btnPlus.Size = new Size(28, 26);
            btnPlus.TabIndex = 2;
            btnPlus.Text = "+";
            btnPlus.UseVisualStyleBackColor = true;
            btnPlus.Click += btnPlus_Click;
            // 
            // CartItemControl
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(pnlQuantity);
            Controls.Add(lblTotalPrice);
            Controls.Add(lblUnitPrice);
            Controls.Add(btnDelete);
            Controls.Add(txtNote);
            Controls.Add(lblToppings);
            Controls.Add(lblDetails);
            Controls.Add(lblProductName);
            Controls.Add(picProduct);
            Controls.Add(chkSelect);
            Name = "CartItemControl";
            Size = new Size(540, 126);
            ((System.ComponentModel.ISupportInitialize)picProduct).EndInit();
            pnlQuantity.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox chkSelect;
        private PictureBox picProduct;
        private Label lblProductName;
        private Label lblDetails;
        private Label lblToppings;
        private TextBox txtNote;
        private Label btnDelete;
        private Label lblUnitPrice;
        private Label lblTotalPrice;
        private Panel pnlQuantity;
        private Button btnMinus;
        private Label lblQuantity;
        private Button btnPlus;
    }
}