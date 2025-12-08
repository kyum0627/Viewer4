//using IGX.Geometry.Common;
//using IGX.Geometry.ConvexHull;
//using IGX.Geometry.GeometryBuilder;
//using OpenTK.Mathematics;

//namespace IGX.Loader.AMFileLoader
//{
//    public class Part
//    {
//        public int node_id;            // for assembly tree navigation, VIZZARD
//        public List<PrimitiveBase> volumes;
//        public AABB3 abb;
//        public OOBB3 obb;

//        public string panelname;       // panel name
//        public string sys_name;        // AM system에서의 Piece Part 이름
//        public string blk_no;          // assembly level 1, block name, assembly name, ...
//        public string unit_blk;        // assembly level 2
//        public string sub_assy;        // assembly level 3
//        public string part_no;         // assembly level 4
//        public string destination;     // Destination
//        //public string fullname;        // full name of piece part
//        public string structureGroup;       // Piece Part Type
//        public int positionInAM;       // Position number as used in AVEVA Marine
//        public string proftype;        // profile type
//        public int profileDesc;        // Profile Volume Description
//        public string grade;           // steel grade
//        public string surftreatment;   // 표면처리
//        public string part_type;
//        public string part_type_dev;     // 시스템에서 가공한 part type

//        public bool bDuplicated;

//        public float surfacearea = 0;
//        public float partvolume = 0;
//        public float thickness;

//        public bool is_hullpart = false;
//        public string symmetry;
//        public string position;
//        public string lot_no;
//        public string work_stage;

//        public string filename;
//        public string fullPath;

//        public string parentNode;
//        public List<string> allChildNodes;

//        public string fab; //

//        public string port;
//        public string starboard;
//        public string pe;
//        public string assyBLk;

//        public string AMType = string.Empty;
//        public List<Vector3> CornerPoints = new List<Vector3>();
//        public string AmApNode;
//        public string Desig = string.Empty;
//        public string HLOC = string.Empty;
//        public List<string> CutCode = new List<string>();
//        public string Zplate = string.Empty;

//        /// <summary>
//        /// ATX file의 volume ID, 음수이면 hole
//        /// </summary>
//        public int IDvolume // Volume ID, 음수이면 hole
//        {
//            get { return volumes[0].InstanceData.GeometryID; }
//        }

//        public AABB3 Bound
//        {
//            get
//            {
//                if (volumes.Count == 0)
//                    return AABB3.Empty;
//                else
//                {
//                    return abb = GetBound();
//                }
//            }
//        }

//        public Part(string pnlname, ObjectGroup og)
//        {
//            panelname = pnlname;
//            sys_name = "";  // Piece Part 이름
//            blk_no = "*";
//            unit_blk = "";
//            sub_assy = "";
//            part_no = "";

//            destination = "";     // SRT 06
//            //fullname = "";      // full name of piece part
//            structureGroup = og.ToString();  // Object Group
//            positionInAM = 0;     // SRT 07, Position number as used in AVEVA Marine
//            proftype = "";        // profile type
//            profileDesc = 0;      // SRT 16, profile 단면 형상
//            grade = "";           // SRT 08, 강재종류
//            surftreatment = "";    // SRT 05

//            bDuplicated = false;
//            surfacearea = 0;
//            partvolume = 0;

//            volumes = new List<PrimitiveBase>(); // added by 2021.01.04
//            abb = AABB3.Empty;
//            fullPath = "";

//            AMType = "";
//            CornerPoints = new List<Vector3>();
//            AmApNode = "";

//            Desig = string.Empty;
//            HLOC = string.Empty;
//            CutCode = new List<string>();
//        }

//        /// <summary>
//        /// Struct Part 생성자
//        /// </summary>
//        public Part(string pnlname, string prtname)
//        {
//            panelname = pnlname;
//            sys_name = prtname;  // Piece Part 이름
//            blk_no = "*";
//            unit_blk = "";
//            sub_assy = "";
//            part_no = "";

//            destination = "";     // SRT 06
//            //fullname = "";      // full name of piece part
//            //structureGroup = "";  // Object Group
//            positionInAM = 0;     // SRT 07, Position number as used in AVEVA Marine
//            proftype = "";        // profile type
//            profileDesc = 0;      // SRT 16, profile 단면 형상
//            grade = "";           // SRT 08, 강재종류
//            surftreatment = "";    // SRT 05

//            bDuplicated = false;
//            surfacearea = 0;
//            partvolume = 0;

//            volumes = new List<PrimitiveBase>(); // added by 2021.01.04
//            abb = AABB3.Empty;
//            fullPath = "";

//            AMType = "";
//            CornerPoints = new List<Vector3>();
//            AmApNode = "";

//            Desig = string.Empty;
//            HLOC = string.Empty;
//            CutCode = new List<string>();
//        }

//        public void semiClear()
//        {

//            this.panelname = string.Empty;
//            //this.sys_name = string.Empty;  // Piece Part 이름
//            this.lot_no = string.Empty;
//            this.blk_no = string.Empty;
//            this.unit_blk = string.Empty;
//            this.sub_assy = string.Empty;
//            this.part_no = string.Empty;

//            this.destination = string.Empty;   // SRT 06

//            this.structureGroup = string.Empty;      // Piece Part Type
//            this.positionInAM = -1;       // SRT 07, Position number as used in AVEVA Marine
//            this.proftype = string.Empty;      // profile type
//            this.profileDesc = -1;    // SRT 16, profile 단면 형상
//            this.grade = string.Empty;  // SRT 08, 강재종류
//            this.surftreatment = string.Empty;  // SRT 05

//            this.port = string.Empty;
//            this.starboard = string.Empty;
//            this.fullPath = string.Empty;

//            //this.AMTYPE = string.Empty;
//            //this.CornerPoints = new List<Vector3>();
//            //this.AmApNode = string.Empty;
//        }

//        //public Part Copy()
//        //{
//        //    Part copied = new Part("", "");

//        //    for (int i = 0; i < volumes.Count; i++)
//        //    {
//        //        PrimitiveBase copyvol = volumes[i].Copy();
//        //        copyvol.part = copied;
//        //        copied.volumes.Add(copyvol);
//        //    }

//        //    copied.abb = new AABB3(abb);
//        //    copied.obb = new OOBB3(obb.center, obb.axisX, obb.axisY, obb.axisZ, obb.extent);
//        //    copied.panelname = panelname;
//        //    copied.sys_name = sys_name;  // Piece Part 이름
//        //    copied.blk_no = blk_no;
//        //    copied.unit_blk = unit_blk;
//        //    copied.sub_assy = sub_assy;
//        //    copied.part_no = part_no;

//        //    copied.destination = destination;   // SRT 06
//        //    //copied.fullname = fullname;      // full name of piece part
//        //    copied.structureGroup = structureGroup;      // Piece Part Type
//        //    copied.positionInAM = positionInAM;       // SRT 07, Position number as used in AVEVA Marine
//        //    copied.proftype = proftype;      // profile type
//        //    copied.profileDesc = profileDesc;    // SRT 16, profile 단면 형상
//        //    copied.grade = grade;  // SRT 08, 강재종류
//        //    copied.surftreatment = surftreatment;  // SRT 05

//        //    copied.bDuplicated = bDuplicated;
//        //    copied.surfacearea = surfacearea;
//        //    copied.partvolume = partvolume;
//        //    copied.fullPath = fullPath;

//        //    copied.AMType = AMType;
//        //    copied.CornerPoints = CornerPoints;
//        //    copied.AmApNode = AmApNode;
//        //    copied.Desig = Desig;
//        //    copied.HLOC = HLOC;
//        //    copied.CutCode = CutCode;
//        //    return copied;
//        //}

//        //public PrimitiveBase AddVolume(PrimitiveBase v)
//        //{
//        //    volumes.Add(v);
//        //    abb = GetBound();
//        //    obb = GetBoundObb();
//        //    return v;
//        //}

//        //public void Clear()
//        //{
//        //    volumes.Clear();
//        //}

//        //public Part CopyAndFlip(Part from, string postfix)
//        //{
//        //    Part copied = from.Copy();

//        //    copied.panelname = from.panelname + postfix;
//        //    copied.sys_name = from.sys_name + postfix;  // Piece Part 이름
//        //    copied.blk_no = from.blk_no + postfix;
//        //    copied.unit_blk = from.unit_blk + postfix;
//        //    copied.sub_assy = from.sub_assy + postfix;
//        //    copied.part_no = from.part_no + postfix;
//        //    copied.destination = from.destination;   // SRT 06

//        //    copied.Flip();

//        //    return copied;
//        //}

//        //public void Flip()
//        //{
//        //    for (int i = 0; i < volumes.Count; i++)
//        //    {
//        //        volumes[i].Flip();
//        //    }

//        //    abb.Flip();
//        //    obb.Flip();
//        //}

//        //public Color4 GetColor()
//        //{
//        //    Enum.TryParse(structureGroup, out ObjectGroup og);

//        //    switch (og)
//        //    {
//        //        case ObjectGroup.PILLAR: return Color4.Gray;
//        //        case ObjectGroup.PANEL: return Color4.Green;
//        //        case ObjectGroup.PANEL_CONTOUR: return Color4.Green;
//        //        case ObjectGroup.PLATE: return Color4.Green;
//        //        case ObjectGroup.ASSEMBLY: return Color4.Green;
//        //        case ObjectGroup.BRACKET: return Color4.Green;
//        //        case ObjectGroup.BRACKET_PLATE: return Color4.Green;
//        //        case ObjectGroup.FLANGE: return Color4.Green;
//        //        case ObjectGroup.DOUBLING: return Color4.Yellow;
//        //        case ObjectGroup.COLLAR_PLATE: return Color4.Yellow;
//        //        case ObjectGroup.STIFFENER: return Color4.Gray;
//        //        case ObjectGroup.LONGITUDINAL: return Color4.Gray;
//        //        case ObjectGroup.LONGITUDINAL_PART: return Color4.Gray;
//        //        case ObjectGroup.TRANSVERSAL: return Color4.GreenYellow;
//        //        case ObjectGroup.TRANSVERSAL_PART: return Color4.GreenYellow;
//        //        case ObjectGroup.HOLE: return Color4.DarkGray;
//        //        case ObjectGroup.CUTOUT: return Color4.DarkGray;
//        //        case ObjectGroup.NOTCH: return Color4.DarkGray;
//        //        default: return Color4.DarkSlateBlue;// DarkSlateBlue;
//        //    }
//        //}

//        /// <summary>
//        /// bounding abb 생성
//        /// </summary>
//        /// <returns></returns>
//        public AABB3 GetBound()
//        {
//            if (volumes.Count == 0)
//                return AABB3.Empty;

//            foreach (PrimitiveBase v in volumes)
//            {
//                abb.Contain(v.Aabb);
//            }
//            return abb;
//        }

//        /// <summary>
//        /// bounding obb 생성
//        /// </summary>
//        /// <returns></returns>
//        public OOBB3 GetBoundObb()
//        {
//            if (volumes.Count == 0)
//                return OOBB3.Empty;

//            obb = volumes[0].Oobb;
//            return obb;
//        }

//        /// <summary>
//        /// key번째 volume
//        /// </summary>
//        /// <param name="key"></param>
//        /// <returns></returns>
//        public PrimitiveBase this[int key]
//        {
//            get { return volumes[key]; }
//            set { volumes[key] = value; }
//        }

//        public List<string> ToRev()
//        {
//            string sysname = this.sys_name;
//            sysname = (this.sys_name == "") ? "noname" : this.sys_name;
//            List<string> txt = new List<string>
//            {
//                "CNTB", // tree start
//                String.Format("{0,6} {1,6}", 1, 2),
//                sysname, //"/" + this.sys_name,
//                String.Format("{0,14:F2} {1,14:F2} {2,14:F2}", abb.GeometryCenter.X, abb.GeometryCenter.Y, abb.GeometryCenter.Z), // center point
//                String.Format("{0,6}", 5) // color
//            };

//            foreach (PrimitiveBase v in volumes)
//                txt.AddRange(v.ToRev());

//            txt.Add("CNTE"); // tree end
//            txt.Add(String.Format("{0,6} {1,6}", 1, 2));
//            return txt;
//        }

//        //// ---- for standard 

//        //public int CompareTo(Part B)
//        //{
//        //    if (B == null)
//        //        return 1;
//        //    else
//        //        return IDvolume.CompareTo(B.IDvolume);
//        //}

//        //public bool Equals(Part other)
//        //{
//        //    if (other == null)
//        //        return false;
//        //    return
//        //        (volumes.Equals(other.volumes));
//        //}

//        //public override bool Equals(object obj)
//        //{
//        //    if (obj == null)
//        //        return false;
//        //    if (!(obj is Part objAsPart))
//        //        return false;
//        //    else
//        //        return Equals(objAsPart);
//        //}

//        //public override int GetHashCode()
//        //{
//        //    unchecked
//        //    {
//        //        int hash = (int)2166136261;
//        //        hash = (hash * 16777619) ^ volumes.GetHashCode();
//        //        return hash;
//        //    }
//        //}

//        //public int SortByNameAscending(string name1, string name2)
//        //{
//        //    return name1.CompareTo(name2);
//        //}

//        //public bool IsCollideOOBB(Part other, float tol = DefaultWeldTolerance.Fillet)
//        //{
//        //    foreach (PrimitiveBase ivol in this.volumes)
//        //    {
//        //        foreach (PrimitiveBase jvol in other.volumes)
//        //        {
//        //            if (ivol.Oobb.Collide(jvol.Oobb, DefaultWeldTolerance.Fillet))
//        //            {
//        //                return true;
//        //            }
//        //            //hschoi : 20240710
//        //            //obb.Collide()함수가 정상적으로 체크하지 못할 경우 CollideA()함수로 한번더 체크함.
//        //            else if (ivol.Oobb.Collide(jvol.Oobb, DefaultWeldTolerance.Fillet))
//        //            {
//        //                return true;
//        //            }
//        //        }
//        //    }
//        //    return false;
//        //}
//    }
//}
