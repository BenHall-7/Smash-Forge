﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using WeifenLuo.WinFormsUI.Docking;
using OpenTK;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Threading;
using Microsoft.VisualBasic.Devices;

namespace Smash_Forge
{
    public partial class MainForm : Form
    {

        public static MainForm Instance
        {
            get { return _instance != null ? _instance : (_instance = new MainForm()); }
        }

        private static MainForm _instance;

        public WorkspaceManager Workspace { get; set; }

        public String[] filesToOpen = null;


        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ThreadStart t = new ThreadStart(Smash_Forge.Update.CheckLatest);
            Thread thread = new Thread(t);
            thread.Start();
            Runtime.renderDepth = 2500f;

            Runtime.renderBones = true;
            Runtime.renderLVD = true;
            Runtime.renderFloor = true;
            Runtime.renderBackGround = true;
            Runtime.renderHitboxes = true;
            Runtime.renderModel = true;
            Runtime.renderPath = true;
            Runtime.renderCollisions = true;
            Runtime.renderCollisionNormals = false;
            Runtime.renderGeneralPoints = true;
            Runtime.renderItemSpawners = true;
            Runtime.renderSpawns = true;
            Runtime.renderRespawns = true;
            Runtime.renderOtherLVDEntries = true;
            Runtime.renderNormals = true;
            Runtime.renderVertColor = true;
            Runtime.renderSwag = false;
            Runtime.renderHurtboxes = true;
            Runtime.renderHurtboxesZone = true;
            Runtime.renderType = Runtime.RenderTypes.Texture;

            // load up the shaders
            Shader cub = new Shader();
            cub.vertexShader(RenderTools.cubevs);
            cub.fragmentShader(RenderTools.cubefs);
            Runtime.shaders.Add("SkyBox", cub);

            Shader poi = new Shader();
            poi.vertexShader(RenderTools.vs_Point);
            poi.fragmentShader(RenderTools.fs_Point);
            Runtime.shaders.Add("Point", poi);

            Shader nud = new Shader();
            nud.vertexShader(VBNViewport.vs);
            nud.fragmentShader(VBNViewport.fs);
            Runtime.shaders.Add("NUD", nud);

            RenderTools.Setup();
        }

        public void openFiles()
        {
            //for (int i = 0; i < filesToOpen.Length; i++)
            //{
            //    string file = filesToOpen[i];
            //    if (file.Equals("--clean"))
            //    {
            //        clearWorkspaceToolStripMenuItem_Click(new object(), new EventArgs());
            //        cleanPreset(new object(), new EventArgs());
            //    }
            //    else if (file.Equals("--superclean"))
            //    {
            //        clearWorkspaceToolStripMenuItem_Click(new object(), new EventArgs());
            //        superCleanPreset(new object(), new EventArgs());
            //    }
            //    else if (file.Equals("--preview"))
            //    {
            //        string chr_00 = filesToOpen[i + 1];
            //        string chr_11 = filesToOpen[i + 2];
            //        string chr_13 = filesToOpen[i + 3];
            //        string stock_90 = filesToOpen[i + 4];
            //        NUT chr_00_nut = null, chr_11_nut = null, chr_13_nut = null, stock_90_nut = null;
            //        if (!chr_00.Equals("blank"))
            //        {
            //            chr_00_nut = new NUT(chr_00);
            //            Runtime.TextureContainers.Add(chr_00_nut);
            //        }
            //        if (!chr_11.Equals("blank"))
            //        {
            //            chr_11_nut = new NUT(chr_11);
            //            Runtime.TextureContainers.Add(chr_11_nut);
            //        }
            //        if (!chr_13.Equals("blank"))
            //        {
            //            chr_13_nut = new NUT(chr_13);
            //            Runtime.TextureContainers.Add(chr_13_nut);
            //        }
            //        if (!stock_90.Equals("blank"))
            //        {
            //            stock_90_nut = new NUT(stock_90);
            //            Runtime.TextureContainers.Add(stock_90_nut);
            //        }
            //        UIPreview uiPreview = new UIPreview(chr_00_nut, chr_11_nut, chr_13_nut, stock_90_nut);
            //        uiPreview.ShowHint = DockState.DockRight;
            //        dockPanel1.DockRightPortion = 270;
            //        AddDockedControl(uiPreview);
            //        i += 4;
            //    }
            //    else
            //    {
            //        openFile(file);
            //    }
            //}
            //filesToOpen = null;
        }

        private void MainForm_Close(object sender, EventArgs e)
        {
            if (Runtime.TargetNUD != null)
                Runtime.TargetNUD.Destroy();

            foreach (ModelContainer n in Runtime.ModelContainers)
            {
                n.Destroy();
            }
            foreach (NUT n in Runtime.TextureContainers)
            {
                n.Destroy();
            }
        }

        public void AddDockedControl(DockContent content)
        {
            if (dockPanel1.DocumentStyle == DocumentStyle.SystemMdi)
            {
                content.MdiParent = this;
                content.Show();
            }
            else
                content.Show(dockPanel1);
        }

        #region Members

        public AnimListPanel animList = new AnimListPanel() { ShowHint = DockState.DockRight };
        public BoneTreePanel boneTreePanel = new BoneTreePanel() { ShowHint = DockState.DockLeft };
        public static TreeNode animNode = new TreeNode("Bone Animations");
        public TreeNode mtaNode = new TreeNode("Material Animations");
        public ProjectTree project = new ProjectTree() { ShowHint = DockState.DockLeft };
        public LVDList lvdList = new LVDList() { ShowHint = DockState.DockLeft };
        public LVDEditor lvdEditor = new LVDEditor() { ShowHint = DockState.DockRight };
        public List<PARAMEditor> paramEditors = new List<PARAMEditor>() { };
        public List<MTAEditor> mtaEditors = new List<MTAEditor>() { };
        public List<ACMDEditor> ACMDEditors = new List<ACMDEditor>() { };
        public List<SwagEditor> SwagEditors = new List<SwagEditor>() { };
        public MeshList meshList = new MeshList() { ShowHint = DockState.DockRight };
        public List<VBNViewport> viewports = new List<VBNViewport>() { new VBNViewport() }; // Default viewport (may mess up with more or less?)
        public NUTEditor nutEditor = null;
        public NUS3BANKEditor nusEditor = null;
        public _3DSTexEditor texEditor = null;

        #endregion

        #region ToolStripMenu

        private void openNUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PARAMEditor currentParam = null;
            ACMDEditor currentACMD = null;
            SwagEditor currentSwagEditor = null;
            foreach (PARAMEditor p in paramEditors)
                if (p.ContainsFocus)
                    currentParam = p;

            foreach (ACMDEditor a in ACMDEditors)
                if (a.ContainsFocus)
                    currentACMD = a;

            foreach (SwagEditor s in SwagEditors)
                if (s.ContainsFocus)
                    currentSwagEditor = s;

            if (currentParam != null)
                currentParam.saveAs();
            else if (currentACMD != null)
                currentACMD.save();
            else if (currentSwagEditor != null)
                currentSwagEditor.save();
            else
            {
                string filename = "";
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Supported Filetypes (VBN,LVD,DAE,DAT)|*.vbn;*.lvd;*.dae;*.dat;|Smash 4 Boneset|*.vbn|All files(*.*)|*.*";
                DialogResult result = save.ShowDialog();

                if (result == DialogResult.OK)
                {
                    filename = save.FileName;
                    saveFile(filename);
                    //OMO.createOMO (anim, vbn, "C:\\Users\\ploaj_000\\Desktop\\WebGL\\test_outut.omo", -1, -1);
                }
            }
        }

        private void openNud(string filename, string name = "")
        {
            string[] files = Directory.GetFiles(System.IO.Path.GetDirectoryName(filename));

            string pnud = filename;
            string pnut = "";
            string pjtb = "";
            string pvbn = "";
            string pmta = "";
            string psb = "";
            string pmoi = "";
            List<string> pacs = new List<string>();

            foreach (string s in files)
            {
                if (s.EndsWith(".nut"))
                    pnut = s;
                if (s.EndsWith(".vbn"))
                    pvbn = s;
                if (s.EndsWith(".jtb"))
                    pjtb = s;
                if (s.EndsWith(".mta"))
                    pmta = s;
                if (s.EndsWith(".sb"))
                    psb = s;
                if (s.EndsWith(".moi"))
                    pmoi = s;
                if (s.EndsWith(".pac"))
                    pacs.Add(s);
            }

            ModelContainer model = new ModelContainer();
            model.name = name;
            if (!pvbn.Equals(""))
            {
                model.vbn = new VBN(pvbn);
                Runtime.TargetVBN = model.vbn;
                if (!pjtb.Equals(""))
                    model.vbn.readJointTable(pjtb);
                if (!psb.Equals(""))
                    model.vbn.swingBones.Read(psb);
            }

            if (!pnut.Equals(""))
            {
                NUT nut = new NUT(pnut);
                Runtime.TextureContainers.Add(nut);
            }

            if (!pnud.Equals(""))
            {
                model.nud = new NUD(pnud);

                //AddDockedControl(new NUDMaterialEditor(model.nud.mesh[0].polygons[0].materials));

                foreach (string s in pacs)
                {
                    PAC p = new PAC();
                    p.Read(s);
                    byte[] data;
                    p.Files.TryGetValue("default.mta", out data);
                    if (data != null)
                    {
                        MTA m = new MTA();
                        m.read(new FileData(data));
                        model.nud.applyMTA(m, 0);
                    }
                }
            }

            if (!pmta.Equals(""))
            {
                try
                {
                    model.mta = new MTA();
                    model.mta.Read(pmta);
                    string mtaName = Path.Combine(Path.GetFileName(Path.GetDirectoryName(pmta)), Path.GetFileName(pmta));
                    Console.WriteLine($"MTA Name - {mtaName}");
                    addMaterialAnimation(mtaName, model.mta);
                }
                catch (EndOfStreamException)
                {
                    model.mta = null;
                }
            }

            if (!pmoi.Equals(""))
            {
                model.moi = new MOI(pmoi);
            }

            if (model.nud != null)
            {
                model.nud.MergePoly();
            }

            Runtime.ModelContainers.Add(model);
            meshList.refresh();

            //ModelViewport viewport = new ModelViewport();
            //viewport.draw.Add(model);
            //AddDockedControl(viewport);
        }

        private void addMaterialAnimation(string name, MTA m)
        {
            if (!Runtime.MaterialAnimations.ContainsValue(m) && !Runtime.MaterialAnimations.ContainsKey(name))
                Runtime.MaterialAnimations.Add(name, m);
            viewports[0].loadMTA(m);
            mtaNode.Nodes.Add(name);
        }

        public static void HashMatch()
        {
            csvHashes csv = new csvHashes(Path.Combine(Application.StartupPath, "hashTable.csv"));
            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                if (m.vbn != null)
                {
                    foreach (Bone bone in m.vbn.bones)
                    {
                        for (int i = 0; i < csv.names.Count; i++)
                        {
                            if (csv.names[i] == bone.Text)
                            {
                                bone.boneId = csv.ids[i];
                            }
                        }
                        if (bone.boneId == 0)
                            bone.boneId = Crc32.Compute(bone.Text);
                    }
                }

                if (m.dat_melee != null)
                {
                    foreach (Bone bone in m.dat_melee.bones.bones)
                    {
                        for (int i = 0; i < csv.names.Count; i++)
                        {
                            if (csv.names[i] == bone.Text)
                            {
                                bone.boneId = csv.ids[i];
                            }
                        }
                        if (bone.boneId == 0)
                            bone.boneId = Crc32.Compute(bone.Text);
                    }
                }
                if (m.bch != null)
                {
                    foreach (BCH.BCH_Model mod in m.bch.models)
                    {
                        foreach (Bone bone in mod.skeleton.bones)
                        {
                            for (int i = 0; i < csv.names.Count; i++)
                            {
                                if (csv.names[i] == bone.Text)
                                {
                                    bone.boneId = csv.ids[i];
                                }
                            }
                        }
                    }
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var abt = new About())
            {
                abt.ShowDialog();
            }
        }

        #endregion

        public void openMats(NUD.Polygon poly, string name)
        {
            (new NUDMaterialEditor(poly) { ShowHint = DockState.DockLeft, Text = name }).Show();
        }

        //<summary>
        //Open an animation based on filename
        //</summary>
        public void openAnimation(string filename)
        {

            //Runtime.Animations.Clear();
            if (filename.EndsWith(".mta"))
            {
                MTA mta = new MTA();
                try
                {
                    mta.Read(filename);
                    Runtime.MaterialAnimations.Add(filename, mta);
                    mtaNode.Nodes.Add(filename);
                    MainForm.Instance.viewports[0].loadMTA(mta);
                    Runtime.TargetMTAString = filename;
                }
                catch (EndOfStreamException)
                {
                    mta = null;
                }
            }
            else if (filename.EndsWith(".smd"))
            {
                var anim = new SkelAnimation();
                if (Runtime.TargetVBN == null)
                    Runtime.TargetVBN = new VBN();
                SMD.read(filename, anim, Runtime.TargetVBN);
                boneTreePanel.treeRefresh();
                Runtime.Animations.Add(filename, anim);
                animNode.Nodes.Add(filename);
            }
            else if (filename.EndsWith(".pac"))
            {
                PAC p = new PAC();
                p.Read(filename);

                foreach (var pair in p.Files)
                {
                    if (pair.Key.EndsWith(".omo"))
                    {
                        var anim = OMO.read(new FileData(pair.Value));
                        string AnimName = Regex.Match(pair.Key, @"([A-Z][0-9][0-9])(.*)").Groups[0].ToString();
                        //AnimName = pair.Key;
                        //AnimName = AnimName.Remove(AnimName.Length - 4);
                        //AnimName = AnimName.Insert(3, "_");
                        if (!string.IsNullOrEmpty(AnimName))
                        {
                            if (Runtime.Animations.ContainsKey(AnimName))
                                Runtime.Animations[AnimName].children.Add(anim);
                            else
                            {
                                animNode.Nodes.Add(AnimName);
                                Runtime.Animations.Add(AnimName, anim);
                            }
                        }
                        else
                        {
                            if (Runtime.Animations.ContainsKey(pair.Key))
                                Runtime.Animations[pair.Key].children.Add(anim);
                            else
                            {
                                animNode.Nodes.Add(pair.Key);
                                Runtime.Animations.Add(pair.Key, anim);
                            }
                        }
                    }
                    else if (pair.Key.EndsWith(".mta"))
                    {
                        MTA mta = new MTA();
                        try
                        {
                            if (!Runtime.MaterialAnimations.ContainsKey(pair.Key))
                            {
                                mta.read(new FileData(pair.Value));
                                Runtime.MaterialAnimations.Add(pair.Key, mta);
                                mtaNode.Nodes.Add(pair.Key);
                            }

                            // matching
                            string AnimName =
                                Regex.Match(pair.Key, @"([A-Z][0-9][0-9])(.*)").Groups[0].ToString()
                                    .Replace(".mta", ".omo");
                            if (Runtime.Animations.ContainsKey(AnimName))
                            {
                                Runtime.Animations[AnimName].children.Add(mta);
                            }

                        }
                        catch (EndOfStreamException)
                        {
                            mta = null;
                        }
                    }
                }
            }

            if (filename.EndsWith(".dat"))
            {
                if (!filename.EndsWith("AJ.dat"))
                    MessageBox.Show("Not a DAT animation");
                else
                {
                    if (Runtime.ModelContainers[0].dat_melee == null)
                        MessageBox.Show("Load a DAT model first");
                    else
                        DAT_Animation.LoadAJ(filename, Runtime.ModelContainers[0].dat_melee.bones);
                }

            }
            //if (Runtime.TargetVBN.bones.Count > 0)
            //{
            if (filename.EndsWith(".omo"))
            {
                Runtime.Animations.Add(filename, OMO.read(new FileData(filename)));
                animNode.Nodes.Add(filename);
            }
            if (filename.EndsWith(".chr0"))
            {
                Runtime.Animations.Add(filename, CHR0.read(new FileData(filename), Runtime.TargetVBN));
                animNode.Nodes.Add(filename);
            }
            if (filename.EndsWith(".anim"))
            {
                Runtime.Animations.Add(filename, ANIM.read(filename, Runtime.TargetVBN));
                animNode.Nodes.Add(filename);
            }
        }

        ///<summary>
        /// Save file as if "Save" option was selected
        /// </summary>
        /// <param name="filename"> Filename of file to save</param>
        public void saveFile(string filename)
        {
            if (filename.EndsWith(".vbn"))
            {
                Runtime.TargetVBN.Endian = Endianness.Big;
                if (!checkBox1.Checked)
                    Runtime.TargetVBN.Endian = Endianness.Little;
                Runtime.TargetVBN.Save(filename);
            }

            if (filename.EndsWith(".lvd") && Runtime.TargetLVD != null)
                File.WriteAllBytes(filename, Runtime.TargetLVD.Rebuild());
            else if (filename.EndsWith(".lvd"))
            {
                DAT d = null;
                foreach (ModelContainer c in Runtime.ModelContainers)
                    if (c.dat_melee != null)
                        d = c.dat_melee;
                if (d != null)
                {
                    DialogResult r =
                        MessageBox.Show(
                            "Would you like to save in safe mode?\n(This is not suggested, only use when needed)",
                            "DAT -> LVD safe mode", MessageBoxButtons.YesNo);
                    if (r == DialogResult.Yes)
                    {
                        File.WriteAllBytes(filename, d.toLVD(true).Rebuild());
                    }
                    else if (r == DialogResult.No)
                    {
                        File.WriteAllBytes(filename, d.toLVD(false).Rebuild());
                    }
                }

            }

            if (filename.EndsWith(".dae"))
            {
                if (Runtime.ModelContainers.Count > 0)
                {
                    Collada.Save(filename, Runtime.ModelContainers[0]);
                }
            }

            if (filename.EndsWith(".dat"))
            {
                foreach(ModelContainer mc in Runtime.ModelContainers)
                {
                    if(mc.dat_melee != null)
                    {
                        FileOutput f = new FileOutput();
                        f.writeBytes(File.ReadAllBytes(mc.dat_melee.filename));

                        //foreach (TreeNode node in mc.dat_melee.tree)
                        //    writeDatJobjPositions(node, f);

                        if (mc.dat_melee.spawns != null)
                        {
                            for (int i = 0; i < mc.dat_melee.spawns.Count; i++)
                            {
                                f.writeFloatAt(mc.dat_melee.spawns[i].x / mc.dat_melee.stageScale, mc.dat_melee.spawnOffs[i]);
                                f.writeFloatAt(mc.dat_melee.spawns[i].y / mc.dat_melee.stageScale, mc.dat_melee.spawnOffs[i] + 4);
                                f.writeFloatAt(0, mc.dat_melee.spawnOffs[i] + 8);
                            }
                        }

                        if (mc.dat_melee.respawns != null)
                        {
                            for(int i = 0; i < mc.dat_melee.respawns.Count; i++)
                            {
                                f.writeFloatAt(mc.dat_melee.respawns[i].x / mc.dat_melee.stageScale, mc.dat_melee.respawnOffs[i]);
                                f.writeFloatAt(mc.dat_melee.respawns[i].y / mc.dat_melee.stageScale, mc.dat_melee.respawnOffs[i] + 4);
                                f.writeFloatAt(0, mc.dat_melee.respawnOffs[i] + 8);
                            }
                        }

                        if (mc.dat_melee.itemSpawns != null)
                        {
                            for (int i = 0; i < mc.dat_melee.itemSpawns.Count; i++)
                            {
                                f.writeFloatAt(mc.dat_melee.itemSpawns[i].x / mc.dat_melee.stageScale, mc.dat_melee.itemSpawnOffs[i]);
                                f.writeFloatAt(mc.dat_melee.itemSpawns[i].y / mc.dat_melee.stageScale, mc.dat_melee.itemSpawnOffs[i] + 4);
                                f.writeFloatAt(0, mc.dat_melee.itemSpawnOffs[i] + 8);
                            }
                        }

                        if (mc.dat_melee.targets != null)
                        {
                            for (int i = 0; i < mc.dat_melee.targets.Count; i++)
                            {
                                f.writeFloatAt(mc.dat_melee.targets[i].x / mc.dat_melee.stageScale, mc.dat_melee.targetOffs[i]);
                                f.writeFloatAt(mc.dat_melee.targets[i].y / mc.dat_melee.stageScale, mc.dat_melee.targetOffs[i] + 4);
                                f.writeFloatAt(0, mc.dat_melee.targetOffs[i] + 8);
                            }
                        }

                        if (mc.dat_melee.blastzones != null)
                        {
                            f.writeFloatAt(mc.dat_melee.blastzones.left / mc.dat_melee.stageScale, mc.dat_melee.blastzoneOffs[0]);
                            f.writeFloatAt(mc.dat_melee.blastzones.top / mc.dat_melee.stageScale, mc.dat_melee.blastzoneOffs[0] + 4);
                            f.writeFloatAt(0, mc.dat_melee.blastzoneOffs[0] + 8);

                            f.writeFloatAt(mc.dat_melee.blastzones.right / mc.dat_melee.stageScale, mc.dat_melee.blastzoneOffs[1]);
                            f.writeFloatAt(mc.dat_melee.blastzones.bottom / mc.dat_melee.stageScale, mc.dat_melee.blastzoneOffs[1] + 4);
                            f.writeFloatAt(0, mc.dat_melee.blastzoneOffs[1] + 8);
                        }

                        if (mc.dat_melee.cameraBounds != null)
                        {
                            f.writeFloatAt(mc.dat_melee.cameraBounds.left / mc.dat_melee.stageScale, mc.dat_melee.cameraBoundOffs[0]);
                            f.writeFloatAt(mc.dat_melee.cameraBounds.top / mc.dat_melee.stageScale, mc.dat_melee.cameraBoundOffs[0] + 4);
                            f.writeFloatAt(0, mc.dat_melee.cameraBoundOffs[0] + 8);

                            f.writeFloatAt(mc.dat_melee.cameraBounds.right / mc.dat_melee.stageScale, mc.dat_melee.cameraBoundOffs[1]);
                            f.writeFloatAt(mc.dat_melee.cameraBounds.bottom / mc.dat_melee.stageScale, mc.dat_melee.cameraBoundOffs[1] + 4);
                            f.writeFloatAt(0, mc.dat_melee.cameraBoundOffs[1] + 8);
                        }
                        
                        if (MessageBox.Show("Overwrite collisions?","DAT Saving", MessageBoxButtons.YesNo) == DialogResult.OK && mc.dat_melee.collisions != null)
                        {
                            while(f.pos() % 0x10 != 0)//get it back to being 0x10 alligned if it isn't already
                                f.writeByte(0);
                            
                            f.writeIntAt(f.pos() - 0x20, mc.dat_melee.collisions.vertOffOff);
                            f.writeIntAt(mc.dat_melee.collisions.vertices.Count, mc.dat_melee.collisions.vertOffOff + 4);
                            foreach(Vector2D vert in mc.dat_melee.collisions.vertices)
                            {
                                f.writeFloat(vert.x);
                                f.writeFloat(vert.y);
                            }
                            f.writeIntAt(f.pos() - 0x20, mc.dat_melee.collisions.linkOffOff);
                            f.writeIntAt(mc.dat_melee.collisions.links.Count, mc.dat_melee.collisions.linkOffOff + 4);
                            foreach(DAT.COLL_DATA.Link link in mc.dat_melee.collisions.links)
                            {
                                f.writeShort(link.vertexIndices[0]);
                                f.writeShort(link.vertexIndices[1]);
                                f.writeShort(link.connectors[0]);
                                f.writeShort(link.connectors[1]);
                                f.writeShort(link.idxVertFromLink);
                                f.writeShort(link.idxVertToLink);
                                f.writeShort(link.collisionAngle);
                                f.writeByte(link.flags);
                                f.writeByte(link.material);
                            }
                            f.writeIntAt(f.pos() - 0x20, mc.dat_melee.collisions.polyOffOff);
                            f.writeIntAt(mc.dat_melee.collisions.areaTable.Count, mc.dat_melee.collisions.polyOffOff + 4);
                            //Recalculate "area table" and write it to file
                            foreach (DAT.COLL_DATA.AreaTableEntry ate in mc.dat_melee.collisions.areaTable)
                            {
                                int ceilingCount = 0, floorCount = 0, leftWallCount = 0, rightWallCount = 0;
                                int firstCeiling = -1, firstFloor = -1, firstLeftWall = -1, firstRightWall = -1;
                                float lowX = float.MaxValue, highX = float.MinValue, lowY = float.MaxValue, highY = float.MinValue;
                                for (int i = ate.idxLowestSpot; i < ate.idxLowestSpot + ate.nbLinks && i < mc.dat_melee.collisions.links.Count; i++)
                                {
                                    DAT.COLL_DATA.Link link = mc.dat_melee.collisions.links[i];
                                    
                                    if ((link.collisionAngle & 4) != 0)//left wall
                                    {
                                        leftWallCount++;
                                        if (firstLeftWall == -1)
                                            firstLeftWall = i;
                                    }
                                    if ((link.collisionAngle & 8) != 0)//right wall
                                    {
                                        rightWallCount++;
                                        if (firstRightWall == -1)
                                            firstRightWall = i;
                                    }
                                    if ((link.collisionAngle & 1) != 0)//floor
                                    {
                                        floorCount++;
                                        if (firstFloor == -1)
                                            firstFloor = i;
                                    }
                                    if ((link.collisionAngle & 2) != 0)//ceiling
                                    {
                                        ceilingCount++;
                                        if (firstCeiling == -1)
                                            firstCeiling = i;
                                    }

                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[0]].x < lowX)
                                        lowX = mc.dat_melee.collisions.vertices[link.vertexIndices[0]].x;
                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[0]].x > highX)
                                        highX = mc.dat_melee.collisions.vertices[link.vertexIndices[0]].x;
                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[0]].y < lowY)
                                        lowY = mc.dat_melee.collisions.vertices[link.vertexIndices[0]].y;
                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[0]].y > highY)
                                        highY = mc.dat_melee.collisions.vertices[link.vertexIndices[0]].y;
                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[1]].x < lowX)
                                        lowX = mc.dat_melee.collisions.vertices[link.vertexIndices[1]].x;
                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[1]].x > highX)
                                        highX = mc.dat_melee.collisions.vertices[link.vertexIndices[1]].x;
                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[1]].y < lowY)
                                        lowY = mc.dat_melee.collisions.vertices[link.vertexIndices[1]].y;
                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[1]].y > highY)
                                        highY = mc.dat_melee.collisions.vertices[link.vertexIndices[1]].y;
                                }

                                if (firstCeiling == -1)
                                    firstCeiling = 0;
                                if (firstFloor == -1)
                                    firstFloor = 0;
                                if (firstLeftWall == -1)
                                    firstLeftWall = 0;
                                if (firstRightWall == -1)
                                    firstRightWall = 0;

                                f.writeShort(firstFloor);
                                f.writeShort(floorCount);
                                f.writeShort(firstCeiling);
                                f.writeShort(ceilingCount);
                                f.writeShort(firstLeftWall);
                                f.writeShort(leftWallCount);
                                f.writeShort(firstRightWall);
                                f.writeShort(rightWallCount);
                                f.writeInt(0);
                                f.writeFloat(lowX - 10);
                                f.writeFloat(lowY - 10);
                                f.writeFloat(highX + 10);
                                f.writeFloat(highY + 10);
                                f.writeShort(ate.idxLowestSpot);
                                f.writeShort(ate.nbLinks);
                            }
                        }
                        f.writeIntAt(f.pos(), 0);
                        f.save(filename);
                    }
                }
            }

            if (filename.EndsWith(".nud"))
            {
                if (Runtime.ModelContainers[0].dat_melee != null)
                {
                    ModelContainer m = Runtime.ModelContainers[0].dat_melee.wrapToNUD();
                    m.nud.Save(filename);
                    m.vbn.Save(filename.Replace(".nud", ".vbn"));
                }
                if (Runtime.ModelContainers[0].bch != null)
                {
                    Runtime.ModelContainers[0].bch.mbn.toNUD().Save(filename);
                    Runtime.ModelContainers[0].bch.models[0].skeleton.Save(filename.Replace(".nud", ".vbn"));
                }
            }
            if (filename.EndsWith(".mbn"))
            {
                if (Runtime.ModelContainers[0].nud != null)
                {
                    MBN m = Runtime.ModelContainers[0].nud.toMBN();
                    m.Save(filename);
                }
            }
        }

        ///<summary>
        ///Open a file based on the filename
        ///</summary>
        /// <param name="filename"> Filename of file to open</param>
        public void openFile(string filename)
        {
            if (!filename.EndsWith(".mta") && !filename.EndsWith(".dat") && !filename.EndsWith(".smd"))
                openAnimation(filename);

            if (filename.EndsWith(".vbn"))
            {
                Runtime.TargetVBN = new VBN(filename);

                ModelContainer con = new ModelContainer();
                con.vbn = Runtime.TargetVBN;
                Runtime.ModelContainers.Add(con);

                if (Directory.Exists("Skapon\\"))
                {
                    NUD nud = Skapon.Create(Runtime.TargetVBN);
                    con.nud = nud;
                }
            }

            if (filename.EndsWith(".sb"))
            {
                SB sb = new SB();
                sb.Read(filename);
                SwagEditor swagEditor = new SwagEditor(sb) {ShowHint = DockState.DockRight};
                AddDockedControl(swagEditor);
                SwagEditors.Add(swagEditor);
            }

            if (filename.EndsWith(".dat"))
            {
                if (filename.EndsWith("AJ.dat"))
                {
                    MessageBox.Show("This is animation; load with Animation -> Import");
                    return;
                }
                DAT dat = new DAT();
                dat.filename = filename;
                dat.Read(new FileData(filename));
                ModelContainer c = new ModelContainer();
                Runtime.ModelContainers.Add(c);
                c.dat_melee = dat;
                dat.PreRender();

                HashMatch();

                Runtime.TargetVBN = dat.bones;
                if (dat.collisions != null)//if the dat is a stage
                {
                    DAT_stage_list stageList = new DAT_stage_list(dat) { ShowHint = DockState.DockLeft };
                    AddDockedControl(stageList);
                }
                DAT_TreeView p = new DAT_TreeView() {ShowHint = DockState.DockLeft};
                p.setDAT(dat);
                AddDockedControl(p);
                //Runtime.TargetVBN = dat.bones;
                meshList.refresh();
            }

            if (filename.EndsWith(".nut"))
            {
                Runtime.TextureContainers.Add(new NUT(filename));
                if (nutEditor == null || nutEditor.IsDisposed)
                {
                    nutEditor = new NUTEditor();
                    nutEditor.Show();
                }
                else
                {
                    nutEditor.BringToFront();
                }
                nutEditor.FillForm();
            }

            if (filename.EndsWith(".tex"))
            {
                if (texEditor == null || texEditor.IsDisposed)
                {
                    texEditor = new _3DSTexEditor();
                    texEditor.Show();
                }
                else
                {
                    texEditor.BringToFront();
                }
                texEditor.OpenTEX(filename);
            }

            if (filename.EndsWith(".lvd"))
            {
                Runtime.TargetLVD = new LVD(filename);
                LVD test = Runtime.TargetLVD;
                lvdList.fillList();
            }
            
            if (filename.EndsWith(".nus3bank"))
            {
                NUS3BANK nus = new NUS3BANK();
                nus.Read(filename);
                Runtime.SoundContainers.Add(nus);
                if (nusEditor == null || nusEditor.IsDisposed)
                {
                    nusEditor = new NUS3BANKEditor();
                    nusEditor.Show();
                }
                else
                {
                    nusEditor.BringToFront();
                }
                nusEditor.FillForm();
            }

            if (filename.EndsWith(".wav"))
            {
                WAVE wav = new WAVE();
                wav.Read(filename);
            }

            if (filename.EndsWith(".mta"))
            {
                Runtime.TargetMTA = new MTA();
                Runtime.TargetMTA.Read(filename);
                viewports[0].loadMTA(Runtime.TargetMTA);
                MTAEditor temp = new MTAEditor(Runtime.TargetMTA) {ShowHint = DockState.DockLeft};
                temp.Text = Path.GetFileName(filename);
                AddDockedControl(temp);
                mtaEditors.Add(temp);
            }

            if (filename.EndsWith(".mtable"))
            {
                //project.openACMD(filename);
                Runtime.Moveset = new MovesetManager(filename);
            }
            if (filename.EndsWith(".atkd"))
            {
                AddDockedControl(new ATKD_Editor(new ATKD().Read(filename)));
            }
            if (filename.EndsWith("path.bin"))
            {
                Runtime.TargetPath = new PathBin(filename);
            }
            else if (filename.EndsWith(".bin"))
            {
                FileData f = new FileData(filename);
                if(f.readShort() == 0xFFFF)
                {
                    PARAMEditor p = new PARAMEditor(filename) { ShowHint = DockState.Document };
                    p.Text = Path.GetFileName(filename);
                    AddDockedControl(p);
                    paramEditors.Add(p);
                }
                else if (f.readString(4,4) == "PATH")
                {
                    Runtime.TargetPath = new PathBin(filename);
                }
                else if (f.readString(0,4) == "ATKD")
                {
                    AddDockedControl(new ATKD_Editor(new ATKD().Read(filename)));
                }
                else
                {
                    Runtime.TargetCMR0 = new CMR0();
                    Runtime.TargetCMR0.read(new FileData(filename));
                }
            }

            if (filename.EndsWith(".mdl0"))
            {
                MDL0Bones mdl0 = new MDL0Bones();
                Runtime.TargetVBN = mdl0.GetVBN(new FileData(filename));
            }

            if (filename.EndsWith(".smd"))
            {
                Runtime.TargetVBN = new VBN();
                SMD.read(filename, new SkelAnimation(), Runtime.TargetVBN);
            }

            if (filename.ToLower().EndsWith(".dae"))
            {
                DAEImportSettings m = new DAEImportSettings();
                m.ShowDialog();
                if (m.exitStatus == DAEImportSettings.Opened)
                {
                    ModelContainer con = new ModelContainer();
                    Runtime.ModelContainers.Add(con);

                    // load vbn
                    con.vbn = m.getVBN();

                    Collada.DAEtoNUD(filename, con, m.checkBox5.Checked);

                    // apply settings
                    m.Apply(con.nud);
                    con.nud.MergePoly();

                    meshList.refresh();
                }
            }


            if (filename.ToLower().EndsWith(".obj"))
            {
                ModelViewport vp = new ModelViewport();
                OBJ obj = new OBJ();
                obj.Read(filename);
                vp.draw.Add(new ModelContainer() { nud = obj.toNUD() });
                Runtime.ModelContainers.Add(new ModelContainer() { nud = obj.toNUD() });
                meshList.refresh();
                AddDockedControl(vp);
                /*DAEImportSettings m = new DAEImportSettings();
                m.ShowDialog();
                if (m.exitStatus == DAEImportSettings.Opened)
                {
                    if (Runtime.ModelContainers.Count < 1)
                        Runtime.ModelContainers.Add(new ModelContainer());


                    // apply settings
                    m.Apply(Runtime.ModelContainers[0].nud);
                    Runtime.ModelContainers[0].nud.MergePoly();

                    meshList.refresh();
                }*/
            }

            if (filename.EndsWith(".mbn"))
            {
                MBN m = new MBN();
                m.Read(filename);
                ModelContainer con = new ModelContainer();
                BCH b = new BCH();
                con.bch = b;
                b.mbn = m;
                b.Read(filename.Replace(".mbn", ".bch"));
                Runtime.ModelContainers.Add(con);
            }

            /*if (filename.EndsWith(".bch"))
            {
                ModelContainer con = new ModelContainer();
                BCH b = new BCH();
                b.Read(filename);
                con.bch = b;
                Runtime.ModelContainers.Add(con);
            }*/

            if (filename.EndsWith(".nud"))
            {
                openNud(filename);
            }

            if (filename.EndsWith(".moi"))
            {
                MOI moi = new MOI(filename);
                AddDockedControl(new MOIEditor(moi) {ShowHint = DockState.DockRight});
            }

            if (filename.EndsWith(".drp"))
            {
                DRP drp = new DRP(filename);
                DRPViewer v = new DRPViewer();
                v.treeView1.Nodes.Add(drp);
                v.Show();
                //project.treeView1.Nodes.Add(drp);
                //project.treeView1.Invalidate();
                //project.treeView1.Refresh();
            }

            if (filename.EndsWith(".wrkspc"))
            {
                Workspace = new WorkspaceManager(project);
                Workspace.OpenWorkspace(filename);
            }

            if (Runtime.TargetVBN != null)
            {
                ModelContainer m = new ModelContainer();
                m.vbn = Runtime.TargetVBN;
                Runtime.ModelContainers.Add(m);

                if (filename.EndsWith(".smd"))
                {
                    m.nud = SMD.toNUD(filename);
                    meshList.refresh();
                }

                boneTreePanel.treeRefresh();
            }
            else
            {
                foreach (ModelContainer m in Runtime.ModelContainers)
                {
                    if (m.vbn != null)
                    {
                        Runtime.TargetVBN = Runtime.ModelContainers[0].vbn;
                        break;
                    }
                }
            }
            // Don't want to mess up the project tree if we
            // just set it up already
            if (!filename.EndsWith(".wrkspc"))
                project.fillTree();
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter =
                    "Supported Formats(.vbn, .mdl0, .smd, .nud, .lvd, .bin, .dae, .mta, .wrkspc, .mbn)|*.vbn;*.mdl0;*.smd;*.lvd;*.nud;*.mtable;*.bin;*.dae;*.obj;*.dat;*.mta;*.wrkspc;*.nut;*.sb;*.mbn;*.tex;*.drp;*.nus3bank;*.wav|" +
                    "Smash 4 Boneset (.vbn)|*.vbn|" +
                    "Namco Model (.nud)|*.nud|" +
                    "Smash 4 Level Data (.lvd)|*.lvd|" +
                    "NW4R Model (.mdl0)|*.mdl0|" +
                    "Source Model (.SMD)|*.smd|" +
                    "Smash 4 Parameters (.bin)|*.bin|" +
                    "Collada Model Format (.dae)|*.dae|" +
                    "Wavefront Object (.obj)|*.obj|" +
                    "All files(*.*)|*.*";

                ofd.Multiselect = true;
                // "Namco Universal Data Folder (.NUD)|*.nud|" +

                if (ofd.ShowDialog() == DialogResult.OK)
                    foreach (string filename in ofd.FileNames)
                        openFile(filename);
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                foreach (string filePath in files)
                {
                    openFile(filePath);
                }
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (Smash_Forge.Update.Downloaded &&
                MessageBox.Show(
                    $"Would you like to download the following update?\n{Smash_Forge.Update.DownloadedRelease.Name}\n{Smash_Forge.Update.DownloadedRelease.Body}",
                    "Smash Forge Updater", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Process p = new Process();
                p.StartInfo.FileName = Path.Combine(Application.StartupPath, "updater/ForgeUpdater.exe");
                p.StartInfo.WorkingDirectory = Path.Combine(Application.StartupPath, "updater/");
                p.StartInfo.Arguments = "-i -r";
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                System.Windows.Forms.Application.Exit();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = false;// (MessageBox.Show("Would you like to close Forge? Any and all unsaved work will be lost.", "Close Confirmation" , MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No);
        }
    }
}
