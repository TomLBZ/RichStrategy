
namespace RichStrategy
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.grpDataGeneration = new System.Windows.Forms.GroupBox();
            this.picTestPlot = new System.Windows.Forms.PictureBox();
            this.btnTestPlotDown = new System.Windows.Forms.Button();
            this.lblRandomRange = new System.Windows.Forms.Label();
            this.btnTestPlotUp = new System.Windows.Forms.Button();
            this.btnTestPlotU = new System.Windows.Forms.Button();
            this.btnTestPlotN = new System.Windows.Forms.Button();
            this.grpStrategies = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnDiffInvTime = new System.Windows.Forms.Button();
            this.btnDerivative = new System.Windows.Forms.Button();
            this.btnDoubleTrendlines = new System.Windows.Forms.Button();
            this.grpDataGeneration.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTestPlot)).BeginInit();
            this.grpStrategies.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpDataGeneration
            // 
            this.grpDataGeneration.Controls.Add(this.btnTestPlotU);
            this.grpDataGeneration.Controls.Add(this.btnTestPlotN);
            this.grpDataGeneration.Controls.Add(this.btnTestPlotUp);
            this.grpDataGeneration.Controls.Add(this.picTestPlot);
            this.grpDataGeneration.Controls.Add(this.btnTestPlotDown);
            this.grpDataGeneration.Controls.Add(this.lblRandomRange);
            this.grpDataGeneration.Location = new System.Drawing.Point(12, 12);
            this.grpDataGeneration.Name = "grpDataGeneration";
            this.grpDataGeneration.Size = new System.Drawing.Size(512, 557);
            this.grpDataGeneration.TabIndex = 4;
            this.grpDataGeneration.TabStop = false;
            this.grpDataGeneration.Text = "Data Generation";
            // 
            // picTestPlot
            // 
            this.picTestPlot.Location = new System.Drawing.Point(6, 51);
            this.picTestPlot.Name = "picTestPlot";
            this.picTestPlot.Size = new System.Drawing.Size(500, 500);
            this.picTestPlot.TabIndex = 8;
            this.picTestPlot.TabStop = false;
            // 
            // btnTestPlotDown
            // 
            this.btnTestPlotDown.Location = new System.Drawing.Point(209, 15);
            this.btnTestPlotDown.Name = "btnTestPlotDown";
            this.btnTestPlotDown.Size = new System.Drawing.Size(95, 30);
            this.btnTestPlotDown.TabIndex = 6;
            this.btnTestPlotDown.Text = "Test Plot (Down)";
            this.btnTestPlotDown.UseVisualStyleBackColor = true;
            this.btnTestPlotDown.Click += new System.EventHandler(this.btnTestPlot_Click);
            // 
            // lblRandomRange
            // 
            this.lblRandomRange.AutoSize = true;
            this.lblRandomRange.Location = new System.Drawing.Point(6, 19);
            this.lblRandomRange.Name = "lblRandomRange";
            this.lblRandomRange.Size = new System.Drawing.Size(39, 13);
            this.lblRandomRange.TabIndex = 4;
            this.lblRandomRange.Text = "Max = ";
            // 
            // btnTestPlotUp
            // 
            this.btnTestPlotUp.Location = new System.Drawing.Point(108, 15);
            this.btnTestPlotUp.Name = "btnTestPlotUp";
            this.btnTestPlotUp.Size = new System.Drawing.Size(95, 30);
            this.btnTestPlotUp.TabIndex = 9;
            this.btnTestPlotUp.Text = "Test Plot (Up)";
            this.btnTestPlotUp.UseVisualStyleBackColor = true;
            this.btnTestPlotUp.Click += new System.EventHandler(this.btnTestPlot_Click);
            // 
            // btnTestPlotU
            // 
            this.btnTestPlotU.Location = new System.Drawing.Point(310, 15);
            this.btnTestPlotU.Name = "btnTestPlotU";
            this.btnTestPlotU.Size = new System.Drawing.Size(95, 30);
            this.btnTestPlotU.TabIndex = 11;
            this.btnTestPlotU.Text = "Test Plot (U)";
            this.btnTestPlotU.UseVisualStyleBackColor = true;
            this.btnTestPlotU.Click += new System.EventHandler(this.btnTestPlot_Click);
            // 
            // btnTestPlotN
            // 
            this.btnTestPlotN.Location = new System.Drawing.Point(411, 15);
            this.btnTestPlotN.Name = "btnTestPlotN";
            this.btnTestPlotN.Size = new System.Drawing.Size(95, 30);
            this.btnTestPlotN.TabIndex = 10;
            this.btnTestPlotN.Text = "Test Plot (N)";
            this.btnTestPlotN.UseVisualStyleBackColor = true;
            this.btnTestPlotN.Click += new System.EventHandler(this.btnTestPlot_Click);
            // 
            // grpStrategies
            // 
            this.grpStrategies.Controls.Add(this.btnDoubleTrendlines);
            this.grpStrategies.Controls.Add(this.btnDerivative);
            this.grpStrategies.Controls.Add(this.btnDiffInvTime);
            this.grpStrategies.Controls.Add(this.label1);
            this.grpStrategies.Location = new System.Drawing.Point(530, 12);
            this.grpStrategies.Name = "grpStrategies";
            this.grpStrategies.Size = new System.Drawing.Size(242, 557);
            this.grpStrategies.TabIndex = 5;
            this.grpStrategies.TabStop = false;
            this.grpStrategies.Text = "Strategies";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // btnDiffInvTime
            // 
            this.btnDiffInvTime.Location = new System.Drawing.Point(6, 51);
            this.btnDiffInvTime.Name = "btnDiffInvTime";
            this.btnDiffInvTime.Size = new System.Drawing.Size(230, 40);
            this.btnDiffInvTime.TabIndex = 1;
            this.btnDiffInvTime.Text = "Difference - Inverse Time";
            this.btnDiffInvTime.UseVisualStyleBackColor = true;
            this.btnDiffInvTime.Click += new System.EventHandler(this.btnDiffInvTime_Click);
            // 
            // btnDerivative
            // 
            this.btnDerivative.Location = new System.Drawing.Point(6, 97);
            this.btnDerivative.Name = "btnDerivative";
            this.btnDerivative.Size = new System.Drawing.Size(230, 40);
            this.btnDerivative.TabIndex = 2;
            this.btnDerivative.Text = "Direvative";
            this.btnDerivative.UseVisualStyleBackColor = true;
            this.btnDerivative.Click += new System.EventHandler(this.btnDerivative_Click);
            // 
            // btnDoubleTrendlines
            // 
            this.btnDoubleTrendlines.Location = new System.Drawing.Point(6, 143);
            this.btnDoubleTrendlines.Name = "btnDoubleTrendlines";
            this.btnDoubleTrendlines.Size = new System.Drawing.Size(230, 40);
            this.btnDoubleTrendlines.TabIndex = 3;
            this.btnDoubleTrendlines.Text = "Double Trendlines";
            this.btnDoubleTrendlines.UseVisualStyleBackColor = true;
            this.btnDoubleTrendlines.Click += new System.EventHandler(this.btnDoubleTrendlines_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 581);
            this.Controls.Add(this.grpStrategies);
            this.Controls.Add(this.grpDataGeneration);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.Text = "RichStrategy";
            this.grpDataGeneration.ResumeLayout(false);
            this.grpDataGeneration.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTestPlot)).EndInit();
            this.grpStrategies.ResumeLayout(false);
            this.grpStrategies.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpDataGeneration;
        private System.Windows.Forms.PictureBox picTestPlot;
        private System.Windows.Forms.Button btnTestPlotDown;
        private System.Windows.Forms.Label lblRandomRange;
        private System.Windows.Forms.Button btnTestPlotUp;
        private System.Windows.Forms.Button btnTestPlotU;
        private System.Windows.Forms.Button btnTestPlotN;
        private System.Windows.Forms.GroupBox grpStrategies;
        private System.Windows.Forms.Button btnDoubleTrendlines;
        private System.Windows.Forms.Button btnDerivative;
        private System.Windows.Forms.Button btnDiffInvTime;
        private System.Windows.Forms.Label label1;
    }
}

