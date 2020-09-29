using MasterOnline.Models;

namespace MasterOnline
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ErasoftContext : DbContext
    {
        public ErasoftContext()
            : base("name=ErasoftContext")
        {
        }
        
        public ErasoftContext(string dbSourceEra, string dbPathEra)
            : base($"Server={dbSourceEra};initial catalog={dbPathEra};" +
                   $"user id=sa;password=admin123^;multipleactiveresultsets=True;" +
                   $"application name=EntityFramework")
        {
        }

        public virtual DbSet<Promosi> PROMOSI { get; set; }
        public virtual DbSet<DetailPromosi> DETAILPROMOSI { get; set; }
        public virtual DbSet<APF01> APF01 { get; set; }
        public virtual DbSet<APF01A> APF01A { get; set; }
        public virtual DbSet<APF02> APF02 { get; set; }
        public virtual DbSet<APF02A> APF02A { get; set; }
        public virtual DbSet<APF02B> APF02B { get; set; }
        public virtual DbSet<APF03> APF03 { get; set; }
        public virtual DbSet<APF03A> APF03A { get; set; }
        public virtual DbSet<APF04> APF04 { get; set; }
        public virtual DbSet<APF05> APF05 { get; set; }
        public virtual DbSet<APF05A> APF05A { get; set; }
        public virtual DbSet<APF11> APF11 { get; set; }
        public virtual DbSet<APFSY> APFSYS { get; set; }
        public virtual DbSet<APLINK> APLINKs { get; set; }
        public virtual DbSet<APT01A> APT01A { get; set; }
        public virtual DbSet<APT01B> APT01B { get; set; }
        public virtual DbSet<APT01C> APT01C { get; set; }
        public virtual DbSet<APT01D> APT01D { get; set; }
        public virtual DbSet<APT02> APT02 { get; set; }
        public virtual DbSet<APT02B> APT02B { get; set; }
        public virtual DbSet<APT02C> APT02C { get; set; }
        public virtual DbSet<APT03A> APT03A { get; set; }
        public virtual DbSet<APT03B> APT03B { get; set; }
        public virtual DbSet<APT03C> APT03C { get; set; }
        public virtual DbSet<APT04A> APT04A { get; set; }
        public virtual DbSet<APT04B> APT04B { get; set; }
        public virtual DbSet<APURUT> APURUTs { get; set; }
        public virtual DbSet<ARF01> ARF01 { get; set; }
        public virtual DbSet<ARF01A> ARF01A { get; set; }
        public virtual DbSet<ARF01B> ARF01B { get; set; }
        public virtual DbSet<ARF01C> ARF01C { get; set; }
        public virtual DbSet<ARF02> ARF02 { get; set; }
        public virtual DbSet<ARF02A> ARF02A { get; set; }
        public virtual DbSet<ARF02B> ARF02B { get; set; }
        public virtual DbSet<ARF03> ARF03 { get; set; }
        public virtual DbSet<ARF03A> ARF03A { get; set; }
        public virtual DbSet<ARF04> ARF04 { get; set; }
        public virtual DbSet<ARF05> ARF05 { get; set; }
        public virtual DbSet<ARF05A> ARF05A { get; set; }
        public virtual DbSet<ARF06A> ARF06A { get; set; }
        public virtual DbSet<ARF06B> ARF06B { get; set; }
        public virtual DbSet<ARF07> ARF07 { get; set; }
        public virtual DbSet<ARF08> ARF08 { get; set; }
        public virtual DbSet<ARF08B> ARF08B { get; set; }
        public virtual DbSet<ARF10> ARF10 { get; set; }
        public virtual DbSet<ARF11> ARF11 { get; set; }
        public virtual DbSet<ARF12> ARF12 { get; set; }
        public virtual DbSet<ARF13> ARF13 { get; set; }
        public virtual DbSet<ARF14> ARF14 { get; set; }
        public virtual DbSet<ARF15> ARF15 { get; set; }
        public virtual DbSet<ARF16> ARF16 { get; set; }
        public virtual DbSet<ARF17> ARF17 { get; set; }
        public virtual DbSet<ARF19> ARF19 { get; set; }
        public virtual DbSet<ARF20> ARF20 { get; set; }
        public virtual DbSet<ARF22> ARF22 { get; set; }
        public virtual DbSet<ARFSY> ARFSYS { get; set; }
        public virtual DbSet<ARLINK> ARLINKs { get; set; }
        public virtual DbSet<ART01A> ART01A { get; set; }
        public virtual DbSet<ART01B> ART01B { get; set; }
        public virtual DbSet<ART01C> ART01C { get; set; }
        public virtual DbSet<ART01D> ART01D { get; set; }
        public virtual DbSet<ART02> ART02 { get; set; }
        public virtual DbSet<ART02B> ART02B { get; set; }
        public virtual DbSet<ART02C> ART02C { get; set; }
        public virtual DbSet<ART03A> ART03A { get; set; }
        public virtual DbSet<ART03B> ART03B { get; set; }
        public virtual DbSet<ART03C> ART03C { get; set; }
        public virtual DbSet<ART04A> ART04A { get; set; }
        public virtual DbSet<ART04B> ART04B { get; set; }
        public virtual DbSet<ART04C> ART04C { get; set; }
        public virtual DbSet<ARURUT> ARURUTs { get; set; }
        public virtual DbSet<dtproperty> dtproperties { get; set; }
        public virtual DbSet<GLBatchTable> GLBatchTables { get; set; }
        public virtual DbSet<GLF002> GLF002 { get; set; }
        public virtual DbSet<GLFAUTO1> GLFAUTO1 { get; set; }
        public virtual DbSet<GLFAUTO2> GLFAUTO2 { get; set; }
        public virtual DbSet<GLFBATCH> GLFBATCHes { get; set; }
        public virtual DbSet<GLFBIAYA> GLFBIAYAs { get; set; }
        public virtual DbSet<GLFBUD> GLFBUDs { get; set; }
        public virtual DbSet<GLFBUD_TEMP> GLFBUD_TEMP { get; set; }
        public virtual DbSet<GLFBUK> GLFBUKs { get; set; }
        public virtual DbSet<GLFCAB> GLFCABs { get; set; }
        public virtual DbSet<GLFCAB_A> GLFCAB_A { get; set; }
        public virtual DbSet<GLFCL> GLFCLS { get; set; }
        public virtual DbSet<GLFDEPT> GLFDEPTs { get; set; }
        public virtual DbSet<GLFDNE> GLFDNEs { get; set; }
        public virtual DbSet<GLFDRL> GLFDRLs { get; set; }
        public virtual DbSet<GLFELI> GLFELIs { get; set; }
        public virtual DbSet<GLFFIXED> GLFFIXEDs { get; set; }
        public virtual DbSet<GLFGRAP1> GLFGRAP1 { get; set; }
        public virtual DbSet<GLFGRAP2> GLFGRAP2 { get; set; }
        public virtual DbSet<GLFJTTAP> GLFJTTAPs { get; set; }
        public virtual DbSet<GLFJUR> GLFJURs { get; set; }
        public virtual DbSet<GLFKEL> GLFKELs { get; set; }
        public virtual DbSet<GLFLAMP1> GLFLAMP1 { get; set; }
        public virtual DbSet<GLFLAMP2> GLFLAMP2 { get; set; }
        public virtual DbSet<GLFLAMP3> GLFLAMP3 { get; set; }
        public virtual DbSet<GLFLAMP4> GLFLAMP4 { get; set; }
        public virtual DbSet<GLFLARI> GLFLARIS { get; set; }
        public virtual DbSet<GLFLEVEL> GLFLEVELs { get; set; }
        public virtual DbSet<GLFMDL1> GLFMDL1 { get; set; }
        public virtual DbSet<GLFMDL2> GLFMDL2 { get; set; }
        public virtual DbSet<GLFMDL3> GLFMDL3 { get; set; }
        public virtual DbSet<GLFMTL> GLFMTLs { get; set; }
        public virtual DbSet<GLFMUT> GLFMUTs { get; set; }
        public virtual DbSet<GLFMUT2> GLFMUT2 { get; set; }
        public virtual DbSet<GLFMUT3> GLFMUT3 { get; set; }
        public virtual DbSet<GLFNER> GLFNERs { get; set; }
        public virtual DbSet<GLFNER1> GLFNER1 { get; set; }
        public virtual DbSet<GLFNER2> GLFNER2 { get; set; }
        public virtual DbSet<GLFNER3> GLFNER3 { get; set; }
        public virtual DbSet<GLFNER4> GLFNER4 { get; set; }
        public virtual DbSet<GLFREK> GLFREKs { get; set; }
        public virtual DbSet<GLFREK_A> GLFREK_A { get; set; }
        public virtual DbSet<GLFREK2> GLFREK2 { get; set; }
        public virtual DbSet<GLFREKTEMP> GLFREKTEMPs { get; set; }
        public virtual DbSet<GLFREV> GLFREVs { get; set; }
        public virtual DbSet<GLFRLA1> GLFRLA1 { get; set; }
        public virtual DbSet<GLFRLA2> GLFRLA2 { get; set; }
        public virtual DbSet<GLFRLA3> GLFRLA3 { get; set; }
        public virtual DbSet<GLFRLA4> GLFRLA4 { get; set; }
        public virtual DbSet<GLFSTTAP> GLFSTTAPs { get; set; }
        public virtual DbSet<GLFSY> GLFSYS { get; set; }
        public virtual DbSet<GLFSYS_old> GLFSYS_old { get; set; }
        public virtual DbSet<GLFTAWAL> GLFTAWALs { get; set; }
        public virtual DbSet<GLFTEMP> GLFTEMPs { get; set; }
        public virtual DbSet<GLFTEMP1> GLFTEMP1 { get; set; }
        public virtual DbSet<GLFTEMP2> GLFTEMP2 { get; set; }
        public virtual DbSet<GLFTEMP3> GLFTEMP3 { get; set; }
        public virtual DbSet<GLFTLINK1> GLFTLINK1 { get; set; }
        public virtual DbSet<GLFTLINK2> GLFTLINK2 { get; set; }
        public virtual DbSet<GLFTRAN1> GLFTRAN1 { get; set; }
        public virtual DbSet<GLFTRAN1TEMP> GLFTRAN1TEMP { get; set; }
        public virtual DbSet<GLFTRAN2> GLFTRAN2 { get; set; }
        public virtual DbSet<GLFTRAN2TEMP> GLFTRAN2TEMP { get; set; }
        public virtual DbSet<GLFVAL> GLFVALs { get; set; }
        public virtual DbSet<GLFVAL2> GLFVAL2 { get; set; }
        public virtual DbSet<GLRF01> GLRF01 { get; set; }
        public virtual DbSet<GLRF02> GLRF02 { get; set; }
        public virtual DbSet<INQUERYPP> INQUERYPPs { get; set; }
        public virtual DbSet<INQUERYP> INQUERYPS { get; set; }
        public virtual DbSet<PBF01> PBF01 { get; set; }
        public virtual DbSet<PBF02> PBF02 { get; set; }
        public virtual DbSet<PBF03> PBF03 { get; set; }
        public virtual DbSet<PBF04> PBF04 { get; set; }
        public virtual DbSet<PBF05> PBF05 { get; set; }
        public virtual DbSet<PBFLOCK> PBFLOCKs { get; set; }
        public virtual DbSet<PBFSY> PBFSYS { get; set; }
        public virtual DbSet<PBT01A> PBT01A { get; set; }
        public virtual DbSet<PBT01B> PBT01B { get; set; }
        public virtual DbSet<PBT01B1> PBT01B1 { get; set; }
        public virtual DbSet<PBT01C> PBT01C { get; set; }
        public virtual DbSet<PBT01D> PBT01D { get; set; }
        public virtual DbSet<PBT01E> PBT01E { get; set; }
        public virtual DbSet<PBT01F> PBT01F { get; set; }
        public virtual DbSet<PBT01G> PBT01G { get; set; }
        public virtual DbSet<PBT01H> PBT01H { get; set; }
        public virtual DbSet<PBT01I> PBT01I { get; set; }
        public virtual DbSet<PBT02> PBT02 { get; set; }
        public virtual DbSet<PBT03> PBT03 { get; set; }
        public virtual DbSet<POF01> POF01 { get; set; }
        public virtual DbSet<POF02> POF02 { get; set; }
        public virtual DbSet<POF02A> POF02A { get; set; }
        public virtual DbSet<POF03> POF03 { get; set; }
        public virtual DbSet<POF03A> POF03A { get; set; }
        public virtual DbSet<POF04> POF04 { get; set; }
        public virtual DbSet<POF05> POF05 { get; set; }
        public virtual DbSet<POF06A> POF06A { get; set; }
        public virtual DbSet<POF06B> POF06B { get; set; }
        public virtual DbSet<POF07> POF07 { get; set; }
        public virtual DbSet<POF08> POF08 { get; set; }
        public virtual DbSet<POF09> POF09 { get; set; }
        public virtual DbSet<POF10> POF10 { get; set; }
        public virtual DbSet<POF11> POF11 { get; set; }
        public virtual DbSet<POF11B> POF11B { get; set; }
        public virtual DbSet<POF12> POF12 { get; set; }
        public virtual DbSet<POF12B> POF12B { get; set; }
        public virtual DbSet<POF13> POF13 { get; set; }
        public virtual DbSet<POF14A> POF14A { get; set; }
        public virtual DbSet<POF14B> POF14B { get; set; }
        public virtual DbSet<POF15> POF15 { get; set; }
        public virtual DbSet<POF16A> POF16A { get; set; }
        public virtual DbSet<POF16B> POF16B { get; set; }
        public virtual DbSet<POF17A> POF17A { get; set; }
        public virtual DbSet<POF17B> POF17B { get; set; }
        public virtual DbSet<POF18> POF18 { get; set; }
        public virtual DbSet<POF19> POF19 { get; set; }
        public virtual DbSet<POF20> POF20 { get; set; }
        public virtual DbSet<POF21A> POF21A { get; set; }
        public virtual DbSet<POF21B> POF21B { get; set; }
        public virtual DbSet<POF22A> POF22A { get; set; }
        public virtual DbSet<POF22B> POF22B { get; set; }
        public virtual DbSet<POF24> POF24 { get; set; }
        public virtual DbSet<POF25> POF25 { get; set; }
        public virtual DbSet<POFLOCK01> POFLOCK01 { get; set; }
        public virtual DbSet<POFLOCK02> POFLOCK02 { get; set; }
        public virtual DbSet<POFLOCK03> POFLOCK03 { get; set; }
        public virtual DbSet<POFSY> POFSYS { get; set; }
        public virtual DbSet<POT01A> POT01A { get; set; }
        public virtual DbSet<POT01B> POT01B { get; set; }
        public virtual DbSet<POT01B1> POT01B1 { get; set; }
        public virtual DbSet<POT01C> POT01C { get; set; }
        public virtual DbSet<POT01D> POT01D { get; set; }
        public virtual DbSet<POT01E> POT01E { get; set; }
        public virtual DbSet<POT01F> POT01F { get; set; }
        public virtual DbSet<POT01G> POT01G { get; set; }
        public virtual DbSet<POT01H> POT01H { get; set; }
        public virtual DbSet<POT02A> POT02A { get; set; }
        public virtual DbSet<POT02B> POT02B { get; set; }
        public virtual DbSet<POT02C> POT02C { get; set; }
        public virtual DbSet<POT02E> POT02E { get; set; }
        public virtual DbSet<POT02F> POT02F { get; set; }
        public virtual DbSet<POT03> POT03 { get; set; }
        public virtual DbSet<POT03A> POT03A { get; set; }
        public virtual DbSet<POT03B> POT03B { get; set; }
        public virtual DbSet<POT03C> POT03C { get; set; }
        public virtual DbSet<POT03D> POT03D { get; set; }
        public virtual DbSet<POT04A> POT04A { get; set; }
        public virtual DbSet<POT04B> POT04B { get; set; }
        public virtual DbSet<POT04C> POT04C { get; set; }
        public virtual DbSet<POT04D> POT04D { get; set; }
        public virtual DbSet<POT05A> POT05A { get; set; }
        public virtual DbSet<POT05B> POT05B { get; set; }
        public virtual DbSet<POT05C> POT05C { get; set; }
        public virtual DbSet<POT05D> POT05D { get; set; }
        public virtual DbSet<SDF08> SDF08 { get; set; }
        public virtual DbSet<SIF01> SIF01 { get; set; }
        public virtual DbSet<SIF02> SIF02 { get; set; }
        public virtual DbSet<SIF03> SIF03 { get; set; }
        public virtual DbSet<SIF04> SIF04 { get; set; }
        public virtual DbSet<SIF05> SIF05 { get; set; }
        public virtual DbSet<SIF06> SIF06 { get; set; }
        public virtual DbSet<SIF07> SIF07 { get; set; }
        public virtual DbSet<SIF08A> SIF08A { get; set; }
        public virtual DbSet<SIF08B> SIF08B { get; set; }
        public virtual DbSet<SIF09> SIF09 { get; set; }
        public virtual DbSet<SIF10> SIF10 { get; set; }
        public virtual DbSet<SIF11> SIF11 { get; set; }
        public virtual DbSet<SIF12> SIF12 { get; set; }
        public virtual DbSet<SIF13A> SIF13A { get; set; }
        public virtual DbSet<SIF13B> SIF13B { get; set; }
        public virtual DbSet<SIF14> SIF14 { get; set; }
        public virtual DbSet<SIF15A> SIF15A { get; set; }
        public virtual DbSet<SIF15B> SIF15B { get; set; }
        public virtual DbSet<SIF16A> SIF16A { get; set; }
        public virtual DbSet<SIF16B> SIF16B { get; set; }
        public virtual DbSet<SIF16C> SIF16C { get; set; }
        public virtual DbSet<SIF17A> SIF17A { get; set; }
        public virtual DbSet<SIF17B> SIF17B { get; set; }
        public virtual DbSet<SIF18A> SIF18A { get; set; }
        public virtual DbSet<SIF18B> SIF18B { get; set; }
        public virtual DbSet<SIF22> SIF22 { get; set; }
        public virtual DbSet<SIF23> SIF23 { get; set; }
        public virtual DbSet<SIF24> SIF24 { get; set; }
        public virtual DbSet<SIFLOCK> SIFLOCKs { get; set; }
        public virtual DbSet<SIFSY> SIFSYS { get; set; }
        public virtual DbSet<SIFSYS_TAMBAHAN> SIFSYS_TAMBAHAN { get; set; }
        public virtual DbSet<SIFSYS_DS> SIFSYS_DS { get; set; }
        public virtual DbSet<SIT01A> SIT01A { get; set; }
        public virtual DbSet<SIT01B> SIT01B { get; set; }
        public virtual DbSet<SIT01B1> SIT01B1 { get; set; }
        public virtual DbSet<SIT01C> SIT01C { get; set; }
        public virtual DbSet<SIT01D> SIT01D { get; set; }
        public virtual DbSet<SIT01E> SIT01E { get; set; }
        public virtual DbSet<SIT01F> SIT01F { get; set; }
        public virtual DbSet<SIT01G> SIT01G { get; set; }
        public virtual DbSet<SIT02A> SIT02A { get; set; }
        public virtual DbSet<SIT02B> SIT02B { get; set; }
        public virtual DbSet<SIT02C> SIT02C { get; set; }
        public virtual DbSet<SIT03A> SIT03A { get; set; }
        public virtual DbSet<SIT03B> SIT03B { get; set; }
        public virtual DbSet<SIT03C> SIT03C { get; set; }
        public virtual DbSet<SOF01> SOF01 { get; set; }
        public virtual DbSet<SOF02> SOF02 { get; set; }
        public virtual DbSet<SOF03> SOF03 { get; set; }
        public virtual DbSet<SOFLOCK> SOFLOCKs { get; set; }
        public virtual DbSet<SOFSY> SOFSYS { get; set; }
        public virtual DbSet<SOT01A> SOT01A { get; set; }
        public virtual DbSet<SOT01B> SOT01B { get; set; }
        public virtual DbSet<SOT01B2> SOT01B2 { get; set; }
        public virtual DbSet<SOT01C> SOT01C { get; set; }
        public virtual DbSet<SOT01D> SOT01D { get; set; }
        public virtual DbSet<SOT01E> SOT01E { get; set; }
        public virtual DbSet<SOT01F> SOT01F { get; set; }
        public virtual DbSet<SOT02A> SOT02A { get; set; }
        public virtual DbSet<SOT02B> SOT02B { get; set; }
        public virtual DbSet<SOT02C> SOT02C { get; set; }
        public virtual DbSet<SOT02D> SOT02D { get; set; }
        public virtual DbSet<STF02> STF02 { get; set; }
        public virtual DbSet<STF02A1> STF02A1 { get; set; }
        public virtual DbSet<STF02B> STF02B { get; set; }
        public virtual DbSet<STF02C> STF02C { get; set; }
        public virtual DbSet<STF02D> STF02D { get; set; }
        public virtual DbSet<STF02E> STF02E { get; set; }
        public virtual DbSet<STF02F> STF02F { get; set; }
        public virtual DbSet<STF02G> STF02G { get; set; }
        public virtual DbSet<STF02H> STF02H { get; set; }
        public virtual DbSet<STF03> STF03 { get; set; }
        public virtual DbSet<STF04> STF04 { get; set; }
        public virtual DbSet<STF05> STF05 { get; set; }
        public virtual DbSet<STF05A> STF05A { get; set; }
        public virtual DbSet<STF06> STF06 { get; set; }
        public virtual DbSet<STF07> STF07 { get; set; }
        public virtual DbSet<STF08> STF08 { get; set; }
        public virtual DbSet<STF08A> STF08A { get; set; }
        public virtual DbSet<STF08B> STF08B { get; set; }
        public virtual DbSet<STF09> STF09 { get; set; }
        public virtual DbSet<STF09A> STF09A { get; set; }
        public virtual DbSet<STF09B> STF09B { get; set; }
        public virtual DbSet<STF09C> STF09C { get; set; }
        public virtual DbSet<STF10> STF10 { get; set; }
        public virtual DbSet<STF10B> STF10B { get; set; }
        public virtual DbSet<STF11> STF11 { get; set; }
        public virtual DbSet<STF11B> STF11B { get; set; }
        public virtual DbSet<STF11C> STF11C { get; set; }
        public virtual DbSet<STF11D> STF11D { get; set; }
        public virtual DbSet<STF12> STF12 { get; set; }
        public virtual DbSet<STF13> STF13 { get; set; }
        public virtual DbSet<STF14> STF14 { get; set; }
        public virtual DbSet<STF16> STF16 { get; set; }
        public virtual DbSet<STF18> STF18 { get; set; }
        public virtual DbSet<STF19> STF19 { get; set; }
        public virtual DbSet<STFCAT> STFCATs { get; set; }
        public virtual DbSet<STFLINE> STFLINEs { get; set; }
        public virtual DbSet<STFLOCK01> STFLOCK01 { get; set; }
        public virtual DbSet<STFLOCK02> STFLOCK02 { get; set; }
        public virtual DbSet<STFSY> STFSYS { get; set; }
        public virtual DbSet<STLINK> STLINKs { get; set; }
        public virtual DbSet<STLINK2A> STLINK2A { get; set; }
        public virtual DbSet<STLINK2B> STLINK2B { get; set; }
        public virtual DbSet<STT01A> STT01A { get; set; }
        public virtual DbSet<STT01B> STT01B { get; set; }
        public virtual DbSet<STT01B1> STT01B1 { get; set; }
        public virtual DbSet<STT01C> STT01C { get; set; }
        public virtual DbSet<STT01C1> STT01C1 { get; set; }
        public virtual DbSet<STT01D> STT01D { get; set; }
        public virtual DbSet<STT02> STT02 { get; set; }
        public virtual DbSet<STT02A> STT02A { get; set; }
        public virtual DbSet<STT02B> STT02B { get; set; }
        public virtual DbSet<STT03A> STT03A { get; set; }
        public virtual DbSet<STT03B> STT03B { get; set; }
        public virtual DbSet<STT04A> STT04A { get; set; }
        public virtual DbSet<STT04B> STT04B { get; set; }
        public virtual DbSet<STT04B1> STT04B1 { get; set; }
        public virtual DbSet<STT04C> STT04C { get; set; }
        public virtual DbSet<STT04D> STT04D { get; set; }
        public virtual DbSet<STT05A> STT05A { get; set; }
        public virtual DbSet<STT05B> STT05B { get; set; }
        public virtual DbSet<STT06> STT06 { get; set; }
        public virtual DbSet<STT07A> STT07A { get; set; }
        public virtual DbSet<STT07B> STT07B { get; set; }
        public virtual DbSet<STT07C> STT07C { get; set; }
        public virtual DbSet<STT07D> STT07D { get; set; }
        public virtual DbSet<STURUT> STURUTs { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<TRF03> TRF03 { get; set; }
        public virtual DbSet<PBT03A> PBT03A { get; set; }
        public virtual DbSet<PBT03B> PBT03B { get; set; }
        public virtual DbSet<POSISISTOCK> POSISISTOCKs { get; set; }
        public virtual DbSet<SIF19> SIF19 { get; set; }
        public virtual DbSet<SIF20A> SIF20A { get; set; }
        public virtual DbSet<SIF20B> SIF20B { get; set; }
        public virtual DbSet<SIF21> SIF21 { get; set; }
        public virtual DbSet<SIFSYS_NEW> SIFSYS_NEW { get; set; }
        public virtual DbSet<SKFSY> SKFSYS { get; set; }
        public virtual DbSet<STFSYS_M> STFSYS_M { get; set; }
        public virtual DbSet<TEMP_PBT01A> TEMP_PBT01A { get; set; }
        public virtual DbSet<TEMP_PBT01B> TEMP_PBT01B { get; set; }
        public virtual DbSet<TEMP_PBT01C> TEMP_PBT01C { get; set; }
        public virtual DbSet<TEMP_PBT01D> TEMP_PBT01D { get; set; }
        public virtual DbSet<TEMP_PBT01E> TEMP_PBT01E { get; set; }
        public virtual DbSet<TEMP_PBT01F> TEMP_PBT01F { get; set; }
        public virtual DbSet<TEMP_POST> TEMP_POST { get; set; }
        public virtual DbSet<TEMP_SIT01A> TEMP_SIT01A { get; set; }
        public virtual DbSet<TEMP_SIT01B> TEMP_SIT01B { get; set; }
        public virtual DbSet<TEMP_SIT01C> TEMP_SIT01C { get; set; }
        public virtual DbSet<TEMP_SIT01D> TEMP_SIT01D { get; set; }
        public virtual DbSet<TEMP_SIT01E> TEMP_SIT01E { get; set; }
        public virtual DbSet<TEMP_SIT01F> TEMP_SIT01F { get; set; }
        public virtual DbSet<TEMP_SIT01G> TEMP_SIT01G { get; set; }
        public virtual DbSet<TEMP_STF08> TEMP_STF08 { get; set; }
        public virtual DbSet<TEMP_STF09> TEMP_STF09 { get; set; }
        public virtual DbSet<TEMP_STT01A> TEMP_STT01A { get; set; }
        public virtual DbSet<TEMP_STT01B> TEMP_STT01B { get; set; }
        public virtual DbSet<TEMP_STT01C> TEMP_STT01C { get; set; }
        public virtual DbSet<tmp_STF08> tmp_STF08 { get; set; }
        public virtual DbSet<tmp_STF08A> tmp_STF08A { get; set; }
        public virtual DbSet<tmp_STF09> tmp_STF09 { get; set; }
        public virtual DbSet<tmp_STF09A> tmp_STF09A { get; set; }
        public virtual DbSet<DeliveryTemplateElevenia> DeliveryTemplateElevenia { get; set; }
        public virtual DbSet<PICKUP_POINT_BLIBLI> PICKUP_POINT_BLIBLI { get; set; }
        public virtual DbSet<DELIVERY_PROVIDER_LAZADA> DELIVERY_PROVIDER_LAZADA { get; set; }
        public virtual DbSet<API_LOG_MARKETPLACE> API_LOG_MARKETPLACE { get; set; }
        public virtual DbSet<LOG_IMPORT_FAKTUR> LOG_IMPORT_FAKTUR { get; set; }
        public virtual DbSet<TEMP_BRG_MP> TEMP_BRG_MP { get; set; }
        public virtual DbSet<TEMP_SHOPEE_ORDERS> TEMP_SHOPEE_ORDERS { get; set; }
        public virtual DbSet<TEMP_SHOPEE_ORDERS_ITEM> TEMP_SHOPEE_ORDERS_ITEM { get; set; }
        //add by fauzi for 82cart
        public virtual DbSet<TEMP_82CART_ORDERS> TEMP_82CART_ORDERS { get; set; }
        public virtual DbSet<TEMP_82CART_ORDERS_ITEM> TEMP_82CART_ORDERS_ITEM { get; set; }
        public DbSet<CATEGORY_82CART> Category82Cart { get; set; }
        //end by fauzi for 82cart
        //add by fauzi for shopify
        public virtual DbSet<TEMP_SHOPIFY_ORDERS> TEMP_SHOPIFY_ORDERS { get; set; }
        public virtual DbSet<TEMP_SHOPIFY_ORDERS_ITEM> TEMP_SHOPIFY_ORDERS_ITEM { get; set; }
        //end by fauzi
        //add by fauzi for upload saldo awal upgrade
        public virtual DbSet<TEMP_SALDOAWAL> TEMP_SALDOAWAL { get; set; }
        //end by fauzi for upload saldo awal upgrade

        public virtual DbSet<TEMP_TOKPED_ORDERS> TEMP_TOKPED_ORDERS { get; set; }
        //public virtual DbSet<API_LOG_MARKETPLACE_PER_ITEM> API_LOG_MARKETPLACE_PER_ITEM { get; set; }
        //public virtual DbSet<TEMP_BRG_MP_EXCEL> TEMP_BRG_MP_EXCEL { get; set; }
        public virtual DbSet<STF20> STF20 { get; set; }
        public virtual DbSet<STF20B> STF20B { get; set; }
        public virtual DbSet<STF02I> STF02I { get; set; }
        public virtual DbSet<CATEGORY_JDID> CATEGORY_JDID { get; set; }

        //add by nurul 20/8/2019
        public virtual DbSet<SIT04A> SIT04A { get; set; }
        public virtual DbSet<SIT04B> SIT04B { get; set; }
        //end add by nurul 20/8/2019

        //add by Tri 20/8/2019
        public virtual DbSet<SOT03A> SOT03A { get; set; }
        public virtual DbSet<SOT03B> SOT03B { get; set; }
        public virtual DbSet<SOT03C> SOT03C { get; set; }
        //end add by Tri 20/8/2019

        //add by nurul 9/4/2020
        public virtual DbSet<TABLE_LOG_DETAIL> TABLE_LOG_DETAIL { get; set; }
        public virtual DbSet<TEMP_UPLOAD_EXCEL_BAYAR> TEMP_UPLOAD_EXCEL_BAYAR { get; set; }
        //end add by nurul 9/4/2020

        //add by Tri harga jual massal
        public virtual DbSet<LOG_HARGAJUAL_A> LOG_HARGAJUAL_A { get; set; }
        public virtual DbSet<LOG_HARGAJUAL_B> LOG_HARGAJUAL_B { get; set; }
        //end add by Tri harga jual massal

        //add by otniel 10/9/2020        
        public virtual DbSet<LINKFTP> LINKFTP { get; set; }
        //end by otniel 10/9/2020

        //add by nurul 19/8/2020
        public virtual DbSet<STF03C> STF03C { get; set; }
        //end add by nurul 19/8/2020

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            Database.SetInitializer<ErasoftContext>(null);
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<APF01>()
                .Property(e => e.AL3)
                .IsUnicode(false);

            modelBuilder.Entity<APF01>()
                .Property(e => e.AL4)
                .IsUnicode(false);

            modelBuilder.Entity<APF01>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_1)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_2)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_3)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_4)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_5)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_6)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_7)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_8)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_9)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_10)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_11)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_12)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_13)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_14)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_15)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_16)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_17)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_18)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_19)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.SPESIFIKASI_20)
                .IsUnicode(false);

            modelBuilder.Entity<APF01A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<APF02B>()
                .Property(e => e.NO_GIRO)
                .IsUnicode(false);

            modelBuilder.Entity<APF02B>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<APF02B>()
                .Property(e => e.BUKTI_TERIMA)
                .IsUnicode(false);

            modelBuilder.Entity<APF02B>()
                .Property(e => e.BUKTI_SETOR)
                .IsUnicode(false);

            modelBuilder.Entity<APF02B>()
                .Property(e => e.BUKTI_GANTI)
                .IsUnicode(false);

            modelBuilder.Entity<APF02B>()
                .Property(e => e.GIRO_PENGGANTI)
                .IsUnicode(false);

            modelBuilder.Entity<APF02B>()
                .Property(e => e.ST_GIRO)
                .IsUnicode(false);

            modelBuilder.Entity<APF02B>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<APF02B>()
                .Property(e => e.BUKTI_TOLAK)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_1)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_2)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_3)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_4)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_5)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_6)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_7)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_8)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_9)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_10)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_11)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_12)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_13)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_14)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_15)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_16)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_17)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_18)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_19)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.SPEC_CAPTION_20)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.AUTO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.DB_PATH)
                .IsUnicode(false);

            modelBuilder.Entity<APFSY>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<APLINK>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<APLINK>()
                .Property(e => e.COST)
                .IsUnicode(false);

            modelBuilder.Entity<APLINK>()
                .Property(e => e.PPH22)
                .IsUnicode(false);

            modelBuilder.Entity<APLINK>()
                .Property(e => e.HUTANG_PO)
                .IsUnicode(false);

            modelBuilder.Entity<APLINK>()
                .Property(e => e.PERSEDIAAN_PO)
                .IsUnicode(false);

            modelBuilder.Entity<APLINK>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<APLINK>()
                .Property(e => e.APTemp)
                .IsUnicode(false);

            modelBuilder.Entity<APLINK>()
                .Property(e => e.BIAYA_TITIPAN)
                .IsUnicode(false);

            modelBuilder.Entity<APLINK>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<APT01A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<APT01A>()
                .Property(e => e.RANGKA)
                .IsUnicode(false);

            modelBuilder.Entity<APT01A>()
                .Property(e => e.MESIN)
                .IsUnicode(false);

            modelBuilder.Entity<APT01C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<APT01D>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<APT01D>()
                .Property(e => e.RANGKA)
                .IsUnicode(false);

            modelBuilder.Entity<APT01D>()
                .Property(e => e.MESIN)
                .IsUnicode(false);

            modelBuilder.Entity<APT02>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<APT02B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<APT02C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<APT03A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<APT03B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<APT03C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<APT04A>()
                .Property(e => e.No_SPK)
                .IsUnicode(false);

            modelBuilder.Entity<APT04A>()
                .Property(e => e.Supp)
                .IsUnicode(false);

            modelBuilder.Entity<APT04A>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<APT04B>()
                .Property(e => e.No_SPK)
                .IsUnicode(false);

            modelBuilder.Entity<APT04B>()
                .Property(e => e.Nobuk)
                .IsUnicode(false);

            modelBuilder.Entity<APURUT>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<APURUT>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.AL)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.AL2)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.TLP)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.PERSO)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.NPWP)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.PKP)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.WIL)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.KLINK)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.AL3)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.SLM)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.KDHARGA)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.AL_KIRIM1)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.AL_KIRIM2)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.AL_KIRIM3)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.KD_ANALISA)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Sort1_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Sort2_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Sort3_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Sort4_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Sort5_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Sort1_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Sort2_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Sort3_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Sort4_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Sort5_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Sort6_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Attr1_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Attr2_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Attr3_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Attr4_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Attr1_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Attr2_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Attr3_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Attr4_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Attr5_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.Kode)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.No_Seri_Pajak)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01>()
                .Property(e => e.GD1)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_1)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_2)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_3)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_4)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_5)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_6)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_7)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_8)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_9)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_10)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_11)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_12)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_13)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_14)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_15)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_16)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_17)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_18)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_19)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.SPESIFIKASI_20)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01B>()
                .Property(e => e.Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01B>()
                .Property(e => e.SLM)
                .IsUnicode(false);

            modelBuilder.Entity<ARF01B>()
                .Property(e => e.Kd_Rute)
                .IsUnicode(false);

            modelBuilder.Entity<ARF02>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<ARF02>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ARF02A>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<ARF02A>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ARF02B>()
                .Property(e => e.NO_GIRO)
                .IsUnicode(false);

            modelBuilder.Entity<ARF02B>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<ARF02B>()
                .Property(e => e.BUKTI_TERIMA)
                .IsUnicode(false);

            modelBuilder.Entity<ARF02B>()
                .Property(e => e.BUKTI_SETOR)
                .IsUnicode(false);

            modelBuilder.Entity<ARF02B>()
                .Property(e => e.BUKTI_TOLAK)
                .IsUnicode(false);

            modelBuilder.Entity<ARF02B>()
                .Property(e => e.BUKTI_GANTI)
                .IsUnicode(false);

            modelBuilder.Entity<ARF02B>()
                .Property(e => e.GIRO_PENGGANTI)
                .IsUnicode(false);

            modelBuilder.Entity<ARF02B>()
                .Property(e => e.ST_GIRO)
                .IsUnicode(false);

            modelBuilder.Entity<ARF02B>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ARF03>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<ARF03>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<ARF03>()
                .Property(e => e.DK)
                .IsUnicode(false);

            modelBuilder.Entity<ARF03>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<ARF03>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ARF03>()
                .Property(e => e.FAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<ARF03A>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<ARF03A>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<ARF03A>()
                .Property(e => e.DK)
                .IsUnicode(false);

            modelBuilder.Entity<ARF03A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<ARF03A>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ARF03A>()
                .Property(e => e.FAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<ARF04>()
                .Property(e => e.Faktur)
                .IsUnicode(false);

            modelBuilder.Entity<ARF05>()
                .Property(e => e.No_Kontrak)
                .IsUnicode(false);

            modelBuilder.Entity<ARF05>()
                .Property(e => e.No_Addendum)
                .IsUnicode(false);

            modelBuilder.Entity<ARF05A>()
                .Property(e => e.No_Kontrak)
                .IsUnicode(false);

            modelBuilder.Entity<ARF05A>()
                .Property(e => e.No_Addendum)
                .IsUnicode(false);

            modelBuilder.Entity<ARF06A>()
                .Property(e => e.KodeGroup)
                .IsUnicode(false);

            modelBuilder.Entity<ARF06A>()
                .Property(e => e.NamaGroup)
                .IsUnicode(false);

            modelBuilder.Entity<ARF06A>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<ARF06A>()
                .Property(e => e.Val)
                .IsUnicode(false);

            modelBuilder.Entity<ARF06A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ARF06B>()
                .Property(e => e.KodeGroup)
                .IsUnicode(false);

            modelBuilder.Entity<ARF06B>()
                .Property(e => e.KodeCust)
                .IsUnicode(false);

            modelBuilder.Entity<ARF06B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ARF07>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<ARF07>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<ARF07>()
                .Property(e => e.DK)
                .IsUnicode(false);

            modelBuilder.Entity<ARF07>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<ARF07>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ARF07>()
                .Property(e => e.FAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.SLM)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Jabatan)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Alamat1)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Alamat2)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Telp)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.HP)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Pager)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.KTP)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Sort1_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Sort2_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Sort3_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Sort4_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Sort5_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Sort1_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Sort2_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Sort3_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Sort4_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Sort5_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Sort6_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Sort7_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Attr1_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Attr2_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Attr3_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Attr1_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Attr2_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Attr3_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Attr4_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.Attr5_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08B>()
                .Property(e => e.SLM)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08B>()
                .Property(e => e.Kode)
                .IsUnicode(false);

            modelBuilder.Entity<ARF08B>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<ARF10>()
                .Property(e => e.WIL)
                .IsUnicode(false);

            modelBuilder.Entity<ARF10>()
                .Property(e => e.NAWIL)
                .IsUnicode(false);

            modelBuilder.Entity<ARF10>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ARF11>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ARF11>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<ARF12>()
                .Property(e => e.LEVEL)
                .IsUnicode(false);

            modelBuilder.Entity<ARF12>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<ARF12>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<ARF13>()
                .Property(e => e.LEVEL)
                .IsUnicode(false);

            modelBuilder.Entity<ARF13>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<ARF13>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<ARF13>()
                .Property(e => e.Person)
                .IsUnicode(false);

            modelBuilder.Entity<ARF14>()
                .Property(e => e.Level1)
                .IsUnicode(false);

            modelBuilder.Entity<ARF14>()
                .Property(e => e.Ket1)
                .IsUnicode(false);

            modelBuilder.Entity<ARF14>()
                .Property(e => e.Level2)
                .IsUnicode(false);

            modelBuilder.Entity<ARF14>()
                .Property(e => e.Ket2)
                .IsUnicode(false);

            modelBuilder.Entity<ARF14>()
                .Property(e => e.Level3)
                .IsUnicode(false);

            modelBuilder.Entity<ARF14>()
                .Property(e => e.Ket3)
                .IsUnicode(false);

            modelBuilder.Entity<ARF14>()
                .Property(e => e.Level4)
                .IsUnicode(false);

            modelBuilder.Entity<ARF14>()
                .Property(e => e.Ket4)
                .IsUnicode(false);

            modelBuilder.Entity<ARF14>()
                .Property(e => e.Level5)
                .IsUnicode(false);

            modelBuilder.Entity<ARF14>()
                .Property(e => e.Ket5)
                .IsUnicode(false);

            modelBuilder.Entity<ARF14>()
                .Property(e => e.Level6)
                .IsUnicode(false);

            modelBuilder.Entity<ARF14>()
                .Property(e => e.Ket6)
                .IsUnicode(false);

            modelBuilder.Entity<ARF14>()
                .Property(e => e.Level7)
                .IsUnicode(false);

            modelBuilder.Entity<ARF14>()
                .Property(e => e.Ket7)
                .IsUnicode(false);

            modelBuilder.Entity<ARF15>()
                .Property(e => e.Level1)
                .IsUnicode(false);

            modelBuilder.Entity<ARF15>()
                .Property(e => e.Ket1)
                .IsUnicode(false);

            modelBuilder.Entity<ARF15>()
                .Property(e => e.Level2)
                .IsUnicode(false);

            modelBuilder.Entity<ARF15>()
                .Property(e => e.Ket2)
                .IsUnicode(false);

            modelBuilder.Entity<ARF15>()
                .Property(e => e.Level3)
                .IsUnicode(false);

            modelBuilder.Entity<ARF15>()
                .Property(e => e.Ket3)
                .IsUnicode(false);

            modelBuilder.Entity<ARF15>()
                .Property(e => e.Level4)
                .IsUnicode(false);

            modelBuilder.Entity<ARF15>()
                .Property(e => e.Ket4)
                .IsUnicode(false);

            modelBuilder.Entity<ARF15>()
                .Property(e => e.Level5)
                .IsUnicode(false);

            modelBuilder.Entity<ARF15>()
                .Property(e => e.Ket5)
                .IsUnicode(false);

            modelBuilder.Entity<ARF15>()
                .Property(e => e.Kode)
                .IsUnicode(false);

            modelBuilder.Entity<ARF16>()
                .Property(e => e.LEVEL)
                .IsUnicode(false);

            modelBuilder.Entity<ARF16>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<ARF16>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<ARF17>()
                .Property(e => e.Level1)
                .IsUnicode(false);

            modelBuilder.Entity<ARF17>()
                .Property(e => e.Ket1)
                .IsUnicode(false);

            modelBuilder.Entity<ARF17>()
                .Property(e => e.Level2)
                .IsUnicode(false);

            modelBuilder.Entity<ARF17>()
                .Property(e => e.Ket2)
                .IsUnicode(false);

            modelBuilder.Entity<ARF17>()
                .Property(e => e.Level3)
                .IsUnicode(false);

            modelBuilder.Entity<ARF17>()
                .Property(e => e.Ket3)
                .IsUnicode(false);

            modelBuilder.Entity<ARF17>()
                .Property(e => e.Level4)
                .IsUnicode(false);

            modelBuilder.Entity<ARF17>()
                .Property(e => e.Ket4)
                .IsUnicode(false);

            modelBuilder.Entity<ARF17>()
                .Property(e => e.Level5)
                .IsUnicode(false);

            modelBuilder.Entity<ARF17>()
                .Property(e => e.Ket5)
                .IsUnicode(false);

            modelBuilder.Entity<ARF17>()
                .Property(e => e.Level6)
                .IsUnicode(false);

            modelBuilder.Entity<ARF17>()
                .Property(e => e.Ket6)
                .IsUnicode(false);

            modelBuilder.Entity<ARF19>()
                .Property(e => e.Kode_MTK)
                .IsUnicode(false);

            modelBuilder.Entity<ARF19>()
                .Property(e => e.Nama_MTK)
                .IsUnicode(false);

            modelBuilder.Entity<ARF20>()
                .Property(e => e.Kode_jurusan)
                .IsUnicode(false);

            modelBuilder.Entity<ARF20>()
                .Property(e => e.Nama_jurusan)
                .IsUnicode(false);

            modelBuilder.Entity<ARF22>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.KET1)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.KET2)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.KET3)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.HARUS_LINK_KEGL)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.TANPA_POSTING)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.METODE_SELISIHKURS)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_1)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_2)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_3)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_4)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_5)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_6)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_7)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_8)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_9)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_10)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_11)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_12)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_13)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_14)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_15)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_16)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_17)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_18)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_19)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.SPEC_CAPTION_20)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort1_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort2_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort3_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort4_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort5_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort6_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort7_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort1_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort2_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort3_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort4_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort5_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort1_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort2_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort3_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort4_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort5_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LSort6_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LAttr1_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LAttr2_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LAttr3_Org)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LAttr1_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LAttr2_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LAttr3_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LAttr4_Cust)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LAttr1_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LAttr2_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LAttr3_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LAttr4_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.LAttr5_Area)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.AUTO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.DB_PATH)
                .IsUnicode(false);

            modelBuilder.Entity<ARFSY>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.KLINK)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.KAS)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.BANK)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.PJL)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.PIUT)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.DISC)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.PPN)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.MATERAI)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.POT)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.MUKA)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.GIRO)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.KREDIT)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.DEBET)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.RETUR)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.KURS)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.SEDIA)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.HPP)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.JURNAL)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.COST)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.PPNBM)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.RETENSI)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.PIUT_SEMENTARA)
                .IsUnicode(false);

            modelBuilder.Entity<ARLINK>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ART01A>()
                .Property(e => e.FAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<ART01A>()
                .Property(e => e.KONTAN)
                .IsUnicode(false);

            modelBuilder.Entity<ART01A>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<ART01A>()
                .Property(e => e.SLM)
                .IsUnicode(false);

            modelBuilder.Entity<ART01A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<ART01A>()
                .Property(e => e.F_PAJAK)
                .IsUnicode(false);

            modelBuilder.Entity<ART01A>()
                .Property(e => e.NCUST)
                .IsUnicode(false);

            modelBuilder.Entity<ART01A>()
                .Property(e => e.WIL)
                .IsUnicode(false);

            modelBuilder.Entity<ART01A>()
                .Property(e => e.SO)
                .IsUnicode(false);

            modelBuilder.Entity<ART01A>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<ART01A>()
                .Property(e => e.POST)
                .IsUnicode(false);

            modelBuilder.Entity<ART01A>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ART01A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ART01A>()
                .Property(e => e.RANGKA)
                .IsUnicode(false);

            modelBuilder.Entity<ART01A>()
                .Property(e => e.MESIN)
                .IsUnicode(false);

            modelBuilder.Entity<ART01B>()
                .Property(e => e.FAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<ART01B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<ART01B>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<ART01B>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<ART01B>()
                .Property(e => e.WO)
                .IsUnicode(false);

            modelBuilder.Entity<ART01B>()
                .Property(e => e.SJ)
                .IsUnicode(false);

            modelBuilder.Entity<ART01C>()
                .Property(e => e.FAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<ART01C>()
                .Property(e => e.REK)
                .IsUnicode(false);

            modelBuilder.Entity<ART01C>()
                .Property(e => e.URAIAN)
                .IsUnicode(false);

            modelBuilder.Entity<ART01C>()
                .Property(e => e.DK)
                .IsUnicode(false);

            modelBuilder.Entity<ART01C>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<ART01C>()
                .Property(e => e.COST)
                .IsUnicode(false);

            modelBuilder.Entity<ART01C>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ART01C>()
                .Property(e => e.REK_LAWAN)
                .IsUnicode(false);

            modelBuilder.Entity<ART01C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ART01D>()
                .Property(e => e.FAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<ART01D>()
                .Property(e => e.KONTAN)
                .IsUnicode(false);

            modelBuilder.Entity<ART01D>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<ART01D>()
                .Property(e => e.SLM)
                .IsUnicode(false);

            modelBuilder.Entity<ART01D>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<ART01D>()
                .Property(e => e.F_PAJAK)
                .IsUnicode(false);

            modelBuilder.Entity<ART01D>()
                .Property(e => e.NCUST)
                .IsUnicode(false);

            modelBuilder.Entity<ART01D>()
                .Property(e => e.WIL)
                .IsUnicode(false);

            modelBuilder.Entity<ART01D>()
                .Property(e => e.SO)
                .IsUnicode(false);

            modelBuilder.Entity<ART01D>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<ART01D>()
                .Property(e => e.POST)
                .IsUnicode(false);

            modelBuilder.Entity<ART01D>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ART01D>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ART01D>()
                .Property(e => e.RANGKA)
                .IsUnicode(false);

            modelBuilder.Entity<ART01D>()
                .Property(e => e.MESIN)
                .IsUnicode(false);

            modelBuilder.Entity<ART02>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<ART02>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<ART02>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<ART02>()
                .Property(e => e.POSTING)
                .IsUnicode(false);

            modelBuilder.Entity<ART02>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ART02>()
                .Property(e => e.NCUST)
                .IsUnicode(false);

            modelBuilder.Entity<ART02>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ART02>()
                .HasMany(e => e.ART02B)
                .WithRequired(e => e.ART02)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ART02B>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<ART02B>()
                .Property(e => e.FAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<ART02B>()
                .Property(e => e.DK)
                .IsUnicode(false);

            modelBuilder.Entity<ART02B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ART02C>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<ART02C>()
                .Property(e => e.REK)
                .IsUnicode(false);

            modelBuilder.Entity<ART02C>()
                .Property(e => e.URAIAN)
                .IsUnicode(false);

            modelBuilder.Entity<ART02C>()
                .Property(e => e.DK)
                .IsUnicode(false);

            modelBuilder.Entity<ART02C>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<ART02C>()
                .Property(e => e.COST)
                .IsUnicode(false);

            modelBuilder.Entity<ART02C>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ART02C>()
                .Property(e => e.REK_LAWAN)
                .IsUnicode(false);

            modelBuilder.Entity<ART02C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ART03A>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<ART03A>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<ART03A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<ART03A>()
                .Property(e => e.POSTING)
                .IsUnicode(false);

            modelBuilder.Entity<ART03A>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ART03A>()
                .Property(e => e.NCUST)
                .IsUnicode(false);

            modelBuilder.Entity<ART03A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ART03A>()
                .HasMany(e => e.ART03B)
                .WithRequired(e => e.ART03A)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ART03B>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<ART03B>()
                .Property(e => e.NFAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<ART03B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ART03C>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<ART03C>()
                .Property(e => e.REK)
                .IsUnicode(false);

            modelBuilder.Entity<ART03C>()
                .Property(e => e.URAIAN)
                .IsUnicode(false);

            modelBuilder.Entity<ART03C>()
                .Property(e => e.DK)
                .IsUnicode(false);

            modelBuilder.Entity<ART03C>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<ART03C>()
                .Property(e => e.COST)
                .IsUnicode(false);

            modelBuilder.Entity<ART03C>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ART03C>()
                .Property(e => e.REK_LAWAN)
                .IsUnicode(false);

            modelBuilder.Entity<ART03C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ART04A>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<ART04A>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<ART04A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<ART04A>()
                .Property(e => e.POSTING)
                .IsUnicode(false);

            modelBuilder.Entity<ART04A>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ART04A>()
                .Property(e => e.NCUST)
                .IsUnicode(false);

            modelBuilder.Entity<ART04A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<ART04B>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<ART04B>()
                .Property(e => e.FAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<ART04B>()
                .Property(e => e.DK)
                .IsUnicode(false);

            modelBuilder.Entity<ART04C>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<ART04C>()
                .Property(e => e.REK)
                .IsUnicode(false);

            modelBuilder.Entity<ART04C>()
                .Property(e => e.URAIAN)
                .IsUnicode(false);

            modelBuilder.Entity<ART04C>()
                .Property(e => e.DK)
                .IsUnicode(false);

            modelBuilder.Entity<ART04C>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<ART04C>()
                .Property(e => e.COST)
                .IsUnicode(false);

            modelBuilder.Entity<ART04C>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<ART04C>()
                .Property(e => e.REK_LAWAN)
                .IsUnicode(false);

            modelBuilder.Entity<ARURUT>()
                .Property(e => e.TYPE)
                .IsUnicode(false);

            modelBuilder.Entity<ARURUT>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<ARURUT>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<ARURUT>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<dtproperty>()
                .Property(e => e.property)
                .IsUnicode(false);

            modelBuilder.Entity<dtproperty>()
                .Property(e => e.value)
                .IsUnicode(false);

            modelBuilder.Entity<GLF002>()
                .Property(e => e.Kode_rekening)
                .IsUnicode(false);

            modelBuilder.Entity<GLF002>()
                .Property(e => e.kode_jurnal)
                .IsUnicode(false);

            modelBuilder.Entity<GLF002>()
                .Property(e => e.bukti)
                .IsUnicode(false);

            modelBuilder.Entity<GLF002>()
                .Property(e => e.dk)
                .IsUnicode(false);

            modelBuilder.Entity<GLF002>()
                .Property(e => e.keterangan)
                .IsUnicode(false);

            modelBuilder.Entity<GLF002>()
                .Property(e => e.valid)
                .IsUnicode(false);

            modelBuilder.Entity<GLFAUTO1>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFAUTO1>()
                .Property(e => e.JURNAL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFAUTO1>()
                .Property(e => e.BUKTI_JURNAL_GL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFAUTO1>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<GLFAUTO2>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFAUTO2>()
                .Property(e => e.URAIAN)
                .IsUnicode(false);

            modelBuilder.Entity<GLFAUTO2>()
                .Property(e => e.DK)
                .IsUnicode(false);

            modelBuilder.Entity<GLFAUTO2>()
                .Property(e => e.REK_LAWAN)
                .IsUnicode(false);

            modelBuilder.Entity<GLFAUTO2>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFAUTO2>()
                .Property(e => e.TBIAYA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFAUTO2>()
                .Property(e => e.ST)
                .IsUnicode(false);

            modelBuilder.Entity<GLFAUTO2>()
                .Property(e => e.PROYEK)
                .IsUnicode(false);

            modelBuilder.Entity<GLFAUTO2>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFAUTO2>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBIAYA>()
                .Property(e => e.REK1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBIAYA>()
                .Property(e => e.REK2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBUD>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBUD>()
                .Property(e => e.THN)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBUD>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBUD_TEMP>()
                .Property(e => e.THN)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBUD_TEMP>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBUK>()
                .Property(e => e.KODE_REK)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBUK>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBUK>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBUK>()
                .Property(e => e.DK)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBUK>()
                .Property(e => e.JURNAL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBUK>()
                .Property(e => e.LAWAN)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBUK>()
                .Property(e => e.PROYEK)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBUK>()
                .Property(e => e.TBIAYA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBUK>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFBUK>()
                .Property(e => e.KURS)
                .IsUnicode(false);

            modelBuilder.Entity<GLFCAB>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFCAB>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<GLFCAB_A>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFCL>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFDEPT>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFDEPT>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<GLFDEPT>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<GLFDNE>()
                .Property(e => e.KDN)
                .IsUnicode(false);

            modelBuilder.Entity<GLFDNE>()
                .Property(e => e.KDN_NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFDNE>()
                .Property(e => e.RL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFDNE>()
                .Property(e => e.KDN_DARI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFDNE>()
                .Property(e => e.KDN_SAMPAI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFDRL>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFDRL>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFDRL>()
                .Property(e => e.KODE_DARI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFDRL>()
                .Property(e => e.KODE_SAMPAI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFELI>()
                .Property(e => e.REK)
                .IsUnicode(false);

            modelBuilder.Entity<GLFELI>()
                .Property(e => e.NAMA_REKENING)
                .IsUnicode(false);

            modelBuilder.Entity<GLFELI>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<GLFFIXED>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<GLFFIXED>()
                .Property(e => e.NBRG)
                .IsUnicode(false);

            modelBuilder.Entity<GLFFIXED>()
                .Property(e => e.REK1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFFIXED>()
                .Property(e => e.REK2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFFIXED>()
                .Property(e => e.J_SUSUT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFFIXED>()
                .Property(e => e.REK3)
                .IsUnicode(false);

            modelBuilder.Entity<GLFFIXED>()
                .Property(e => e.JENIS)
                .IsUnicode(false);

            modelBuilder.Entity<GLFFIXED>()
                .Property(e => e.LOKASI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFFIXED>()
                .Property(e => e.KEL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFGRAP1>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFGRAP1>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFGRAP1>()
                .Property(e => e.J_GRAP)
                .IsUnicode(false);

            modelBuilder.Entity<GLFGRAP1>()
                .Property(e => e.REK_AGR1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFGRAP1>()
                .Property(e => e.REK_AGR2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFGRAP1>()
                .Property(e => e.REK1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFGRAP1>()
                .Property(e => e.REK2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFGRAP2>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFGRAP2>()
                .Property(e => e.REK_AGR1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFGRAP2>()
                .Property(e => e.REK_AGR2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFGRAP2>()
                .Property(e => e.REK1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFGRAP2>()
                .Property(e => e.REK2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFJTTAP>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<GLFJTTAP>()
                .Property(e => e.JENIS)
                .IsUnicode(false);

            modelBuilder.Entity<GLFJTTAP>()
                .Property(e => e.REK1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFJTTAP>()
                .Property(e => e.REK2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFJTTAP>()
                .Property(e => e.REK3)
                .IsUnicode(false);

            modelBuilder.Entity<GLFJTTAP>()
                .Property(e => e.REK4)
                .IsUnicode(false);

            modelBuilder.Entity<GLFJTTAP>()
                .Property(e => e.REK5)
                .IsUnicode(false);

            modelBuilder.Entity<GLFJUR>()
                .Property(e => e.jurnal)
                .IsUnicode(false);

            modelBuilder.Entity<GLFJUR>()
                .Property(e => e.njurnal)
                .IsUnicode(false);

            modelBuilder.Entity<GLFJUR>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<GLFKEL>()
                .Property(e => e.KEL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFKEL>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP1>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP1>()
                .Property(e => e.JUDUL1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP1>()
                .Property(e => e.JUDUL2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP1>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP1>()
                .Property(e => e.KODE_BARIS)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP1>()
                .Property(e => e.KODE_KOLOM)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP1>()
                .Property(e => e.CNILAI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP1>()
                .Property(e => e.CASH)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP1>()
                .Property(e => e.TXT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP2>()
                .Property(e => e.KODE_BARIS)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP2>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP2>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP2>()
                .Property(e => e.CETAK_TKEL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP2>()
                .Property(e => e.JR)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP2>()
                .Property(e => e.HTK)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP2>()
                .Property(e => e.CETAK_GTOT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP2>()
                .Property(e => e.NA_GTOT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP2>()
                .Property(e => e.AWAL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP2>()
                .Property(e => e.NAMA_GRANDTOTAL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP2>()
                .Property(e => e.NAMA_GT_KEL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP2>()
                .Property(e => e.HIT_GT_KEL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP2>()
                .Property(e => e.JR_GT_KEL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP2>()
                .Property(e => e.CETAK_GT_KEL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP3>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP3>()
                .Property(e => e.N_RL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP3>()
                .Property(e => e.KODE_DARI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP3>()
                .Property(e => e.KODE_SAMPAI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP3>()
                .Property(e => e.DRDEPT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP3>()
                .Property(e => e.SDDEPT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP3>()
                .Property(e => e.KODE_BARIS)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP3>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP3>()
                .Property(e => e.Kolom_Khusus)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP3>()
                .Property(e => e.DRCOST_CENTER)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP3>()
                .Property(e => e.SDCOST_CENTER)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.KODE_KOLOM)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.PERSEN)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.J1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.J2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.J3)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.J4)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.J5)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.J6)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.J7)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.J8)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.J9)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.J10)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.J11)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.J12)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.J13)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.T1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.T2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.T3)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.T4)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.T5)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.T6)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.T7)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.T8)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.T9)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.T10)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.T11)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.T12)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLAMP4>()
                .Property(e => e.T13)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.REK1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.REK2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.REK3)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.REK4)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.REK5)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.REK6)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.REK7)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.REK8)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.REK9)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.REK10)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.DR1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.DR2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.DR3)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.DR4)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.DR5)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.DR6)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.DR7)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.DR8)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.DR9)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.DR10)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.SD1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.SD2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.SD3)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.SD4)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.SD5)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.SD6)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.SD7)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.SD8)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.SD9)
                .IsUnicode(false);

            modelBuilder.Entity<GLFLARI>()
                .Property(e => e.SD10)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL1>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL1>()
                .Property(e => e.JUDUL1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL1>()
                .Property(e => e.JUDUL2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL1>()
                .Property(e => e.KODE_BARIS)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL1>()
                .Property(e => e.CNILAI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL2>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL2>()
                .Property(e => e.KODE_BARIS)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL2>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL2>()
                .Property(e => e.JENIS)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL2>()
                .Property(e => e.JSALDO)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL2>()
                .Property(e => e.CETAK_TT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL2>()
                .Property(e => e.NAMA_TT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL2>()
                .Property(e => e.AKTIVA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL2>()
                .Property(e => e.CETAK_AKHIR)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL2>()
                .Property(e => e.NAMA_AKHIR)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL2>()
                .HasMany(e => e.GLFMDL3)
                .WithRequired(e => e.GLFMDL2)
                .HasForeignKey(e => new { e.KODE, e.KODE_BARIS })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<GLFMDL3>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL3>()
                .Property(e => e.KODE_DARI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL3>()
                .Property(e => e.KODE_SAMPAI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL3>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMDL3>()
                .Property(e => e.KODE_BARIS)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMTL>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMUT>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMUT2>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFMUT3>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER>()
                .Property(e => e.KKN)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER>()
                .Property(e => e.NKN)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER1>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER1>()
                .Property(e => e.JUDUL1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER1>()
                .Property(e => e.JUDUL2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER1>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER1>()
                .Property(e => e.KODE_BARIS)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER1>()
                .Property(e => e.KODE_KOLOM)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER1>()
                .Property(e => e.CNILAI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER2>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER2>()
                .Property(e => e.KKN)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER2>()
                .Property(e => e.NKN)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER2>()
                .Property(e => e.AKTIVA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER2>()
                .Property(e => e.CETAK_KEL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER2>()
                .Property(e => e.NAMA_KEL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER2>()
                .Property(e => e.Hitung_Kel)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER3>()
                .Property(e => e.KDN_NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER3>()
                .Property(e => e.RL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER3>()
                .Property(e => e.KDN_DARI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER3>()
                .Property(e => e.KDN_SAMPAI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER3>()
                .Property(e => e.PERINCI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER3>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER3>()
                .Property(e => e.KDN)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.KODE_KOLOM)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.PERSEN)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.J1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.J2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.J3)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.J4)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.J5)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.J6)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.J7)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.J8)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.J9)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.J10)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.J11)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.J12)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.J13)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.T1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.T2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.T3)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.T4)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.T5)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.T6)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.T7)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.T8)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.T9)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.T10)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.T11)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.T12)
                .IsUnicode(false);

            modelBuilder.Entity<GLFNER4>()
                .Property(e => e.T13)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK>()
                .Property(e => e.JR)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK>()
                .Property(e => e.type)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK>()
                .Property(e => e.kurs)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK>()
                .Property(e => e.rek_konsol)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK>()
                .Property(e => e.UserId)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK_A>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK_A>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK_A>()
                .Property(e => e.JR)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK_A>()
                .Property(e => e.type)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK_A>()
                .Property(e => e.kurs)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK_A>()
                .Property(e => e.rek_konsol)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK_A>()
                .Property(e => e.UserId)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK2>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK2>()
                .Property(e => e.KODE2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK2>()
                .Property(e => e.JR)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREK2>()
                .Property(e => e.REK_KONSOL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREKTEMP>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREKTEMP>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREKTEMP>()
                .Property(e => e.JR)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREKTEMP>()
                .Property(e => e.type)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREKTEMP>()
                .Property(e => e.kurs)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREKTEMP>()
                .Property(e => e.rek_konsol)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREKTEMP>()
                .Property(e => e.UserId)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREV>()
                .Property(e => e.KURS)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREV>()
                .Property(e => e.REK)
                .IsUnicode(false);

            modelBuilder.Entity<GLFREV>()
                .Property(e => e.FlagSaldoAwal)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA1>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA1>()
                .Property(e => e.JUDUL1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA1>()
                .Property(e => e.JUDUL2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA1>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA1>()
                .Property(e => e.KODE_BARIS)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA1>()
                .Property(e => e.KODE_KOLOM)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA1>()
                .Property(e => e.CNILAI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA1>()
                .Property(e => e.TXT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA2>()
                .Property(e => e.KODE_BARIS)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA2>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA2>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA2>()
                .Property(e => e.CETAK_RL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA2>()
                .Property(e => e.JR)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA2>()
                .Property(e => e.HIT_KEL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA2>()
                .Property(e => e.CETAK_KEL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA2>()
                .Property(e => e.NAKEM)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA2>()
                .Property(e => e.K_PJL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA2>()
                .Property(e => e.NARL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA2>()
                .Property(e => e.NOLKAN)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA2>()
                .Property(e => e.NASUB)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA3>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA3>()
                .Property(e => e.SEDIA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA3>()
                .Property(e => e.AWAL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA3>()
                .Property(e => e.KODE_DARI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA3>()
                .Property(e => e.KODE_SAMPAI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA3>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA3>()
                .Property(e => e.SDEPT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA3>()
                .Property(e => e.COST)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA3>()
                .Property(e => e.SCOST)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA3>()
                .Property(e => e.KODE_BARIS)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA3>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA3>()
                .Property(e => e.PERINCI)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.KODE_KOLOM)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.PERSEN)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.J1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.J2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.J3)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.J4)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.J5)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.J6)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.J7)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.J8)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.J9)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.J10)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.J11)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.J12)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.J13)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.T1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.T2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.T3)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.T4)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.T5)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.T6)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.T7)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.T8)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.T9)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.T10)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.T11)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.T12)
                .IsUnicode(false);

            modelBuilder.Entity<GLFRLA4>()
                .Property(e => e.T13)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSTTAP>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSY>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.ID)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.ALAMAT)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.RL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.CASH)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.J_AKTIVA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.TRM)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.KLR)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.J_UMUM)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.BB)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.appl)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.kurs)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.metoda)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.RekDiTahan)
                .IsUnicode(false);

            modelBuilder.Entity<GLFSYS_old>()
                .Property(e => e.NoSeriLink)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTAWAL>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTAWAL>()
                .Property(e => e.LKS_KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTEMP>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTEMP>()
                .Property(e => e.REK1)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTEMP>()
                .Property(e => e.REK2)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTEMP1>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTEMP1>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTEMP1>()
                .Property(e => e.JR)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTEMP2>()
                .Property(e => e.NO_REK)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTEMP2>()
                .Property(e => e.DK)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTEMP3>()
                .Property(e => e.NO_REK)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK1>()
                .Property(e => e.bukti)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK1>()
                .Property(e => e.jurnal)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK1>()
                .Property(e => e.urai)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK1>()
                .Property(e => e.posting)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK1>()
                .Property(e => e.pcost)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK1>()
                .Property(e => e.userid)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK1>()
                .Property(e => e.BUKTI_AR_AP)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK1>()
                .Property(e => e.BUKTI_JURNAL_GL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK1>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK2>()
                .Property(e => e.rek)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK2>()
                .Property(e => e.urai)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK2>()
                .Property(e => e.dk)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK2>()
                .Property(e => e.kurs)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK2>()
                .Property(e => e.rek_lawan)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK2>()
                .Property(e => e.dept)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK2>()
                .Property(e => e.tbiaya)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK2>()
                .Property(e => e.bukti)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK2>()
                .Property(e => e.proyek)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK2>()
                .Property(e => e.UserId)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK2>()
                .Property(e => e.GST_Type)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTLINK2>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN1>()
                .Property(e => e.bukti)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN1>()
                .Property(e => e.jurnal)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN1>()
                .Property(e => e.urai)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN1>()
                .Property(e => e.posting)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN1>()
                .Property(e => e.pcost)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN1>()
                .Property(e => e.userid)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN1>()
                .Property(e => e.BUKTI_AR_AP)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN1>()
                .Property(e => e.BUKTI_LINK_GL)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN1>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN1TEMP>()
                .Property(e => e.bukti)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN1TEMP>()
                .Property(e => e.jurnal)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN1TEMP>()
                .Property(e => e.urai)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN1TEMP>()
                .Property(e => e.posting)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN1TEMP>()
                .Property(e => e.pcost)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN1TEMP>()
                .Property(e => e.userid)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2>()
                .Property(e => e.bukti)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2>()
                .Property(e => e.rek)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2>()
                .Property(e => e.urai)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2>()
                .Property(e => e.dk)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2>()
                .Property(e => e.kurs)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2>()
                .Property(e => e.rek_lawan)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2>()
                .Property(e => e.dept)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2>()
                .Property(e => e.tbiaya)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2>()
                .Property(e => e.proyek)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2>()
                .Property(e => e.UserId)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2>()
                .Property(e => e.GST_Type)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2TEMP>()
                .Property(e => e.rek)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2TEMP>()
                .Property(e => e.urai)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2TEMP>()
                .Property(e => e.dk)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2TEMP>()
                .Property(e => e.kurs)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2TEMP>()
                .Property(e => e.rek_lawan)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2TEMP>()
                .Property(e => e.dept)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2TEMP>()
                .Property(e => e.tbiaya)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2TEMP>()
                .Property(e => e.bukti)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2TEMP>()
                .Property(e => e.proyek)
                .IsUnicode(false);

            modelBuilder.Entity<GLFTRAN2TEMP>()
                .Property(e => e.UserId)
                .IsUnicode(false);

            modelBuilder.Entity<GLFVAL>()
                .Property(e => e.kurs)
                .IsUnicode(false);

            modelBuilder.Entity<GLFVAL>()
                .Property(e => e.ket)
                .IsUnicode(false);

            modelBuilder.Entity<GLFVAL2>()
                .Property(e => e.KURS)
                .IsUnicode(false);

            modelBuilder.Entity<GLRF01>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<GLRF01>()
                .Property(e => e.X3)
                .IsUnicode(false);

            modelBuilder.Entity<GLRF01>()
                .Property(e => e.X4)
                .IsUnicode(false);

            modelBuilder.Entity<GLRF01>()
                .Property(e => e.X_HITUNG)
                .IsUnicode(false);

            modelBuilder.Entity<GLRF01>()
                .Property(e => e.X_PERIODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLRF01>()
                .Property(e => e.X_BANDING)
                .IsUnicode(false);

            modelBuilder.Entity<GLRF01>()
                .Property(e => e.X_PERSEN)
                .IsUnicode(false);

            modelBuilder.Entity<GLRF01>()
                .Property(e => e.Y3)
                .IsUnicode(false);

            modelBuilder.Entity<GLRF01>()
                .Property(e => e.Y4)
                .IsUnicode(false);

            modelBuilder.Entity<GLRF01>()
                .Property(e => e.Y_HITUNG)
                .IsUnicode(false);

            modelBuilder.Entity<GLRF01>()
                .Property(e => e.Y_PERIODE)
                .IsUnicode(false);

            modelBuilder.Entity<GLRF01>()
                .Property(e => e.Y_BANDING)
                .IsUnicode(false);

            modelBuilder.Entity<GLRF01>()
                .Property(e => e.CETAK)
                .IsUnicode(false);

            modelBuilder.Entity<INQUERYPP>()
                .Property(e => e.POP_NO)
                .IsUnicode(false);

            modelBuilder.Entity<INQUERYPP>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<INQUERYPP>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<INQUERYPP>()
                .Property(e => e.Nama2)
                .IsUnicode(false);

            modelBuilder.Entity<INQUERYPP>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<INQUERYPP>()
                .Property(e => e.CONNECTION_ID)
                .IsUnicode(false);

            modelBuilder.Entity<INQUERYP>()
                .Property(e => e.MATRIX)
                .IsUnicode(false);

            modelBuilder.Entity<INQUERYP>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<INQUERYP>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<INQUERYP>()
                .Property(e => e.Nama2)
                .IsUnicode(false);

            modelBuilder.Entity<INQUERYP>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<INQUERYP>()
                .Property(e => e.CONNECTION_ID)
                .IsUnicode(false);

            modelBuilder.Entity<PBF01>()
                .Property(e => e.BIAYA)
                .IsUnicode(false);

            modelBuilder.Entity<PBF01>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<PBF01>()
                .Property(e => e.REK)
                .IsUnicode(false);

            modelBuilder.Entity<PBF02>()
                .Property(e => e.JENIS)
                .IsUnicode(false);

            modelBuilder.Entity<PBF02>()
                .Property(e => e.SERI)
                .IsUnicode(false);

            modelBuilder.Entity<PBF02>()
                .Property(e => e.KODE_PAJAK)
                .IsUnicode(false);

            modelBuilder.Entity<PBF02>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<PBF03>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<PBF03>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<PBF04>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<PBF04>()
                .Property(e => e.SUPP_PPN)
                .IsUnicode(false);

            modelBuilder.Entity<PBF04>()
                .Property(e => e.NAMA_SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_1)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_2)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_3)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_4)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_5)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_6)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_7)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_8)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_9)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_10)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_11)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_12)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_13)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_14)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_15)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_16)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_17)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_18)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_19)
                .IsUnicode(false);

            modelBuilder.Entity<PBF05>()
                .Property(e => e.CAT_CAPTION_20)
                .IsUnicode(false);

            modelBuilder.Entity<PBFLOCK>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<PBFLOCK>()
                .Property(e => e.INV)
                .IsUnicode(false);

            modelBuilder.Entity<PBFLOCK>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<PBFLOCK>()
                .Property(e => e.MACHINE_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.ADA_PO)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.ADA_PB)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.EDIT_BRG_PO)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.EDIT_BRG_PB)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.TERIMA)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.SERI_PI)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.SERI_RB)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.AUTO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.HPOKOK)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.POSTING_STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.LINK_GL)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.JT_BELI)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.JT_RETURBELI)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.METODA_LINK)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.NS_FAKTUR_PPN)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.TINGKAT_DISC)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.PPNBM)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.MFDesimal)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.QFDesimal)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.DB_PATH)
                .IsUnicode(false);

            modelBuilder.Entity<PBFSY>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.INV)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.JENIS)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.PO)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.POSTING)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.APP)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.F_PAJAK)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.REF)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.NO_INVOICE_SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.WO_SUBCON)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.CONSIGNEE)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.SHIPMENT_TYPE)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.BL_NO)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.SHIPPER)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .Property(e => e.PORT)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01A>()
                .HasMany(e => e.PBT01B)
                .WithRequired(e => e.PBT01A)
                .HasForeignKey(e => new { e.JENISFORM, e.INV })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PBT01A>()
                .HasMany(e => e.PBT01B1)
                .WithRequired(e => e.PBT01A)
                .HasForeignKey(e => new { e.JENISFORM, e.INV })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PBT01A>()
                .HasMany(e => e.PBT01C)
                .WithRequired(e => e.PBT01A)
                .HasForeignKey(e => new { e.JENISFORM, e.INV })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PBT01A>()
                .HasMany(e => e.PBT01D)
                .WithRequired(e => e.PBT01A)
                .HasForeignKey(e => new { e.JENISFORM, e.INV })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PBT01A>()
                .HasOptional(e => e.PBT01F)
                .WithRequired(e => e.PBT01A);

            modelBuilder.Entity<PBT01B>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B>()
                .Property(e => e.INV)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B>()
                .Property(e => e.PO)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B>()
                .Property(e => e.NAMA_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B>()
                .Property(e => e.BK)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B>()
                .Property(e => e.AUTO_LOAD)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B>()
                .Property(e => e.BRG_ORIGINAL)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B>()
                .Property(e => e.LKU)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B>()
                .HasMany(e => e.PBT01E)
                .WithRequired(e => e.PBT01B)
                .HasForeignKey(e => new { e.JENISFORM, e.BUKTI, e.NO_URUT })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PBT01B1>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B1>()
                .Property(e => e.INV)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B1>()
                .Property(e => e.PO)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B1>()
                .Property(e => e.JENIS_SIZE)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B1>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B1>()
                .Property(e => e.BK)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B1>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B1>()
                .Property(e => e.AUTO_LOAD)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B1>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B1>()
                .Property(e => e.SORT1)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B1>()
                .Property(e => e.SORT2)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B1>()
                .Property(e => e.SORT3)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01B1>()
                .Property(e => e.SORT4)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01C>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01C>()
                .Property(e => e.INV)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01C>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01D>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01D>()
                .Property(e => e.INV)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01D>()
                .Property(e => e.BIAYA)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01D>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01D>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01E>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01E>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01E>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01E>()
                .Property(e => e.LOT_NO)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01E>()
                .Property(e => e.BATCH_NO)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01E>()
                .Property(e => e.GUD)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01E>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01E>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01E>()
                .Property(e => e.SPESIFIKASI)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01E>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01F>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01F>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01F>()
                .Property(e => e.CATATAN_1)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01F>()
                .Property(e => e.CATATAN_2)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01F>()
                .Property(e => e.CATATAN_3)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01F>()
                .Property(e => e.CATATAN_4)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01F>()
                .Property(e => e.CATATAN_5)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01F>()
                .Property(e => e.CATATAN_6)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01F>()
                .Property(e => e.CATATAN_7)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01F>()
                .Property(e => e.CATATAN_8)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01F>()
                .Property(e => e.CATATAN_9)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01F>()
                .Property(e => e.CATATAN_10)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01F>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01G>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01G>()
                .Property(e => e.INV)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01G>()
                .Property(e => e.NO_INVOICE)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01G>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.INV)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.PREFIXNO)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.AssetNm)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.SerialNo)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.AssetTp)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.Status)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.Branch)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.Manufacture)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.Model)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.GRP1)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.GRP2)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.GRP3)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.Dept)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.Respon)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.PONo)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.Vlt)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.Supplier)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.Person)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.Phone)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.Method)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.AssetAccNo)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.DeprAccNo)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.CostAccNo)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.HarmonyID)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.upsize_ts)
                .IsFixedLength();

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.Photo_Path)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.DEPT_GL)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.COST_GL)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.PHOTO2)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01H>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01I>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01I>()
                .Property(e => e.INV)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01I>()
                .Property(e => e.NO_INVOICE)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01I>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<PBT01I>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<PBT02>()
                .Property(e => e.PIB)
                .IsUnicode(false);

            modelBuilder.Entity<PBT02>()
                .Property(e => e.NMPKB)
                .IsUnicode(false);

            modelBuilder.Entity<PBT02>()
                .Property(e => e.NPWP)
                .IsUnicode(false);

            modelBuilder.Entity<PBT02>()
                .Property(e => e.INV)
                .IsUnicode(false);

            modelBuilder.Entity<PBT02>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<PBT03>()
                .Property(e => e.INV)
                .IsUnicode(false);

            modelBuilder.Entity<PBT03>()
                .Property(e => e.NMPKB)
                .IsUnicode(false);

            modelBuilder.Entity<PBT03>()
                .Property(e => e.NPWP)
                .IsUnicode(false);

            modelBuilder.Entity<POF01>()
                .Property(e => e.DIVISION_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POF01>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<POF01>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF01>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<POF02>()
                .Property(e => e.DIVISION_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POF02>()
                .Property(e => e.DESC)
                .IsUnicode(false);

            modelBuilder.Entity<POF02>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF02>()
                .HasMany(e => e.POF02A)
                .WithRequired(e => e.POF02)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<POF02A>()
                .Property(e => e.DIVISION_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POF02A>()
                .Property(e => e.ITEM_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POF02A>()
                .Property(e => e.ITEM_DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<POF02A>()
                .Property(e => e.UNIT_CODE)
                .IsUnicode(false);

            modelBuilder.Entity<POF02A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF03>()
                .Property(e => e.ITEM_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POF03>()
                .Property(e => e.SUPPLIER_CODE)
                .IsUnicode(false);

            modelBuilder.Entity<POF03>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF03>()
                .HasMany(e => e.POF03A)
                .WithRequired(e => e.POF03)
                .HasForeignKey(e => new { e.ITEM_NO, e.SUPPLIER_CODE })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<POF03A>()
                .Property(e => e.ITEM_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POF03A>()
                .Property(e => e.SUPPLIER_CODE)
                .IsUnicode(false);

            modelBuilder.Entity<POF03A>()
                .Property(e => e.CURRENCY_CODE)
                .IsUnicode(false);

            modelBuilder.Entity<POF03A>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<POF03A>()
                .Property(e => e.PO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POF03A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_1)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_2)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_3)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_4)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_5)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_6)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_7)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_8)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_9)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_10)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_11)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_12)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_13)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_14)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_15)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_16)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_17)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_18)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_19)
                .IsUnicode(false);

            modelBuilder.Entity<POF04>()
                .Property(e => e.CAT_CAPTION_20)
                .IsUnicode(false);

            modelBuilder.Entity<POF05>()
                .Property(e => e.BUYER_CODE)
                .IsUnicode(false);

            modelBuilder.Entity<POF05>()
                .Property(e => e.BUYER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF05>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF06A>()
                .Property(e => e.BUYER_CODE)
                .IsUnicode(false);

            modelBuilder.Entity<POF06A>()
                .Property(e => e.LEVEL_SORT)
                .IsUnicode(false);

            modelBuilder.Entity<POF06A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF06B>()
                .Property(e => e.BUYER_CODE)
                .IsUnicode(false);

            modelBuilder.Entity<POF06B>()
                .Property(e => e.LEVEL_SORT)
                .IsUnicode(false);

            modelBuilder.Entity<POF06B>()
                .Property(e => e.SORT_FIELD)
                .IsUnicode(false);

            modelBuilder.Entity<POF06B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POF06B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF07>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF07>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<POF07>()
                .Property(e => e.LEVEL)
                .IsUnicode(false);

            modelBuilder.Entity<POF07>()
                .Property(e => e.PARAF)
                .IsUnicode(false);

            modelBuilder.Entity<POF08>()
                .Property(e => e.ALAMAT)
                .IsUnicode(false);

            modelBuilder.Entity<POF08>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<POF08>()
                .Property(e => e.GUD)
                .IsUnicode(false);

            modelBuilder.Entity<POF08>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF09>()
                .Property(e => e.ALASAN)
                .IsUnicode(false);

            modelBuilder.Entity<POF09>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<POF09>()
                .Property(e => e.KET2)
                .IsUnicode(false);

            modelBuilder.Entity<POF09>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF10>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POF10>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<POF10>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF11>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<POF11>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<POF11B>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<POF11B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POF11B>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<POF11B>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<POF11B>()
                .Property(e => e.STN2)
                .IsUnicode(false);

            modelBuilder.Entity<POF11B>()
                .Property(e => e.STN3)
                .IsUnicode(false);

            modelBuilder.Entity<POF11B>()
                .Property(e => e.STN4)
                .IsUnicode(false);

            modelBuilder.Entity<POF12>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<POF12>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<POF12>()
                .Property(e => e.JENIS)
                .IsUnicode(false);

            modelBuilder.Entity<POF12>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF12B>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<POF12B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POF12B>()
                .Property(e => e.JENIS)
                .IsUnicode(false);

            modelBuilder.Entity<POF12B>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<POF12B>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<POF12B>()
                .Property(e => e.STN2)
                .IsUnicode(false);

            modelBuilder.Entity<POF12B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF13>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<POF13>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF14A>()
                .Property(e => e.DIV)
                .IsUnicode(false);

            modelBuilder.Entity<POF14A>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POF14A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF14B>()
                .Property(e => e.DIV)
                .IsUnicode(false);

            modelBuilder.Entity<POF14B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POF14B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF15>()
                .Property(e => e.BIAYA)
                .IsUnicode(false);

            modelBuilder.Entity<POF15>()
                .Property(e => e.KETERANGAN)
                .IsUnicode(false);

            modelBuilder.Entity<POF16A>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POF16A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF16B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POF16B>()
                .Property(e => e.BIAYA)
                .IsUnicode(false);

            modelBuilder.Entity<POF16B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF17A>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<POF17A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF17B>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<POF17B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POF17B>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<POF17B>()
                .Property(e => e.BRG_SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<POF17B>()
                .Property(e => e.NAMA_BRG_SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<POF17B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF18>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<POF18>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<POF18>()
                .Property(e => e.ALAMAT)
                .IsUnicode(false);

            modelBuilder.Entity<POF18>()
                .Property(e => e.FIELD1)
                .IsUnicode(false);

            modelBuilder.Entity<POF18>()
                .Property(e => e.FIELD2)
                .IsUnicode(false);

            modelBuilder.Entity<POF18>()
                .Property(e => e.FIELD3)
                .IsUnicode(false);

            modelBuilder.Entity<POF18>()
                .Property(e => e.FIELD4)
                .IsUnicode(false);

            modelBuilder.Entity<POF18>()
                .Property(e => e.FIELD5)
                .IsUnicode(false);

            modelBuilder.Entity<POF18>()
                .Property(e => e.FIELD6)
                .IsUnicode(false);

            modelBuilder.Entity<POF18>()
                .Property(e => e.FIELD7)
                .IsUnicode(false);

            modelBuilder.Entity<POF18>()
                .Property(e => e.FIELD8)
                .IsUnicode(false);

            modelBuilder.Entity<POF18>()
                .Property(e => e.FIELD9)
                .IsUnicode(false);

            modelBuilder.Entity<POF18>()
                .Property(e => e.FIELD10)
                .IsUnicode(false);

            modelBuilder.Entity<POF19>()
                .Property(e => e.SHIPPER)
                .IsUnicode(false);

            modelBuilder.Entity<POF19>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<POF20>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<POF21A>()
                .Property(e => e.Group)
                .IsUnicode(false);

            modelBuilder.Entity<POF21A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<POF21A>()
                .Property(e => e.PS)
                .IsUnicode(false);

            modelBuilder.Entity<POF21B>()
                .Property(e => e.Group)
                .IsUnicode(false);

            modelBuilder.Entity<POF21B>()
                .Property(e => e.Kobar)
                .IsUnicode(false);

            modelBuilder.Entity<POF22A>()
                .Property(e => e.Supp)
                .IsUnicode(false);

            modelBuilder.Entity<POF22A>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<POF22B>()
                .Property(e => e.Supp)
                .IsUnicode(false);

            modelBuilder.Entity<POF22B>()
                .Property(e => e.KdGroup)
                .IsUnicode(false);

            modelBuilder.Entity<POF24>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POF24>()
                .Property(e => e.LEVEL)
                .IsUnicode(false);

            modelBuilder.Entity<POF24>()
                .Property(e => e.DIVISI)
                .IsUnicode(false);

            modelBuilder.Entity<POF24>()
                .Property(e => e.PARAF)
                .IsUnicode(false);

            modelBuilder.Entity<POF24>()
                .Property(e => e.Al_Gbr)
                .IsUnicode(false);

            modelBuilder.Entity<POF25>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POFLOCK02>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<POFLOCK02>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<POFLOCK02>()
                .Property(e => e.MACHINE_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<POFLOCK03>()
                .Property(e => e.NO_MATRIK)
                .IsUnicode(false);

            modelBuilder.Entity<POFLOCK03>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<POFLOCK03>()
                .Property(e => e.MACHINE_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.AUTONUM)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.HRGSATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.DEFAULTSATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.NO_SERI_PP)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.NO_SERI_PO)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.TERBILANG_IN)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.APPROVE_PP)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.VALIDASI_TOLERANSI)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.VALIDASI_BUYER)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.APPROVE_PO)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.NO_SERI_MATRIK)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.VALIDASI_SO)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.CAP_TYPE1)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.CAP_TYPE2)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.CAP_TYPE3)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.CAP_TYPE4)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.CAP_TYPE5)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.PP_ENTRY_STYLE)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.BIAYA_IMPORT)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.Hutang_PO)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.KD_ALT_KRM)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.PREFIX_CLOSING_PO)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.LEVEL_APPROVAL_PO)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.LEVEL_APPROVAL_PP)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.MFDesimal)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.QFDesimal)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.DB_PATH)
                .IsUnicode(false);

            modelBuilder.Entity<POFSY>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.PO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.SUPPLIER_CODE)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.SUPPLIER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.PO_VALUTA)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.JENIS)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.STATUS_APPROVE)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.USERNAME_APPROVE)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.ALAMAT)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.MATRIX)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.CONSIGNEE)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.SHIPMENT_TYPE)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.BL_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.ORDER_CONFIRMATION)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.MAWB)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.QTY)
                .HasPrecision(18, 0);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.CONTENT)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.SHIPMENT_TERM)
                .HasPrecision(18, 0);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.SHIPPER)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.PORT)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.PROSES_PO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.GROUP_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.STATUS_APPROVE_2)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.USERNAME_APPROVE_2)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.STATUS_APPROVE_3)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.USERNAME_APPROVE_3)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.APPROVE_CATATAN_1)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.APPROVE_CATATAN_2)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.APPROVE_CATATAN_3)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT01A>()
                .HasOptional(e => e.POT01E)
                .WithRequired(e => e.POT01A);

            modelBuilder.Entity<POT01B>()
                .Property(e => e.PO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B>()
                .Property(e => e.ITEM_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B>()
                .Property(e => e.ITEM_DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B>()
                .Property(e => e.POP_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B>()
                .Property(e => e.PS_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B>()
                .HasMany(e => e.POT01C)
                .WithRequired(e => e.POT01B)
                .HasForeignKey(e => new { e.PO_NO, e.NO_URUT_UNIT })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<POT01B>()
                .HasMany(e => e.POT01D)
                .WithRequired(e => e.POT01B)
                .HasForeignKey(e => new { e.PO_NO, e.NO_URUT_UNIT })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<POT01B1>()
                .Property(e => e.PO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B1>()
                .Property(e => e.JENIS_SIZE)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B1>()
                .Property(e => e.POP_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B1>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B1>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B1>()
                .Property(e => e.SORT1)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B1>()
                .Property(e => e.SORT2)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B1>()
                .Property(e => e.SORT3)
                .IsUnicode(false);

            modelBuilder.Entity<POT01B1>()
                .Property(e => e.SORT4)
                .IsUnicode(false);

            modelBuilder.Entity<POT01C>()
                .Property(e => e.PO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01C>()
                .Property(e => e.KODE_BRG_UNIT)
                .IsUnicode(false);

            modelBuilder.Entity<POT01C>()
                .Property(e => e.KODE_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POT01C>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<POT01C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT01D>()
                .Property(e => e.PO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01D>()
                .Property(e => e.GUD)
                .IsUnicode(false);

            modelBuilder.Entity<POT01D>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<POT01D>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT01E>()
                .Property(e => e.PO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01E>()
                .Property(e => e.CATATAN_1)
                .IsUnicode(false);

            modelBuilder.Entity<POT01E>()
                .Property(e => e.CATATAN_2)
                .IsUnicode(false);

            modelBuilder.Entity<POT01E>()
                .Property(e => e.CATATAN_3)
                .IsUnicode(false);

            modelBuilder.Entity<POT01E>()
                .Property(e => e.CATATAN_4)
                .IsUnicode(false);

            modelBuilder.Entity<POT01E>()
                .Property(e => e.CATATAN_5)
                .IsUnicode(false);

            modelBuilder.Entity<POT01E>()
                .Property(e => e.CATATAN_6)
                .IsUnicode(false);

            modelBuilder.Entity<POT01E>()
                .Property(e => e.CATATAN_7)
                .IsUnicode(false);

            modelBuilder.Entity<POT01E>()
                .Property(e => e.CATATAN_8)
                .IsUnicode(false);

            modelBuilder.Entity<POT01E>()
                .Property(e => e.CATATAN_9)
                .IsUnicode(false);

            modelBuilder.Entity<POT01E>()
                .Property(e => e.CATATAN_10)
                .IsUnicode(false);

            modelBuilder.Entity<POT01E>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT01F>()
                .Property(e => e.PO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01F>()
                .Property(e => e.NO_SO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01F>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT01G>()
                .Property(e => e.PO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01G>()
                .Property(e => e.POP_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01G>()
                .Property(e => e.DIVISION)
                .IsUnicode(false);

            modelBuilder.Entity<POT01G>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<POT01G>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.PO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.CONSIGNEE)
                .IsUnicode(false);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.SHIPMENT_TYPE)
                .IsUnicode(false);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.BL_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.ORDER_CONFIRMATION)
                .IsUnicode(false);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.MAWB)
                .IsUnicode(false);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.QTY)
                .HasPrecision(18, 0);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.CONTENT)
                .IsUnicode(false);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.SHIPMENT_TERM)
                .HasPrecision(18, 0);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.FREIGHT)
                .HasPrecision(18, 0);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.INSURANCE)
                .HasPrecision(18, 0);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.OTHERS)
                .HasPrecision(18, 0);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.SHIPPER)
                .IsUnicode(false);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.PORT)
                .IsUnicode(false);

            modelBuilder.Entity<POT01H>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.POP_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.DIVISION)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.APPROVE_OLEH)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.BUYER_CODE)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.PO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.GroupBrg)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.APPROVE_OLEH_2)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.APPROVE_OLEH_3)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.APPROVE_CATATAN_1)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.APPROVE_CATATAN_2)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.APPROVE_CATATAN_3)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT02A>()
                .HasMany(e => e.POT02B)
                .WithRequired(e => e.POT02A)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<POT02A>()
                .HasOptional(e => e.POT02E)
                .WithRequired(e => e.POT02A);

            modelBuilder.Entity<POT02B>()
                .Property(e => e.POP_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT02B>()
                .Property(e => e.ITEM_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT02B>()
                .Property(e => e.ITEM_DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<POT02B>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<POT02B>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT02B>()
                .Property(e => e.KODE_BUYER)
                .IsUnicode(false);

            modelBuilder.Entity<POT02B>()
                .Property(e => e.APPROVE_CATATAN_1)
                .IsUnicode(false);

            modelBuilder.Entity<POT02B>()
                .Property(e => e.APPROVE_CATATAN_2)
                .IsUnicode(false);

            modelBuilder.Entity<POT02B>()
                .Property(e => e.APPROVE_CATATAN_3)
                .IsUnicode(false);

            modelBuilder.Entity<POT02B>()
                .Property(e => e.APPROVE_OLEH)
                .IsUnicode(false);

            modelBuilder.Entity<POT02B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT02B>()
                .HasMany(e => e.POT02C)
                .WithRequired(e => e.POT02B)
                .HasForeignKey(e => new { e.POP_NO, e.NO_URUT_UNIT })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<POT02C>()
                .Property(e => e.POP_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT02C>()
                .Property(e => e.KODE_BRG_UNIT)
                .IsUnicode(false);

            modelBuilder.Entity<POT02C>()
                .Property(e => e.KODE_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POT02C>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<POT02C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT02E>()
                .Property(e => e.POP_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT02E>()
                .Property(e => e.CATATAN_1)
                .IsUnicode(false);

            modelBuilder.Entity<POT02E>()
                .Property(e => e.CATATAN_2)
                .IsUnicode(false);

            modelBuilder.Entity<POT02E>()
                .Property(e => e.CATATAN_3)
                .IsUnicode(false);

            modelBuilder.Entity<POT02E>()
                .Property(e => e.CATATAN_4)
                .IsUnicode(false);

            modelBuilder.Entity<POT02E>()
                .Property(e => e.CATATAN_5)
                .IsUnicode(false);

            modelBuilder.Entity<POT02E>()
                .Property(e => e.CATATAN_6)
                .IsUnicode(false);

            modelBuilder.Entity<POT02E>()
                .Property(e => e.CATATAN_7)
                .IsUnicode(false);

            modelBuilder.Entity<POT02E>()
                .Property(e => e.CATATAN_8)
                .IsUnicode(false);

            modelBuilder.Entity<POT02E>()
                .Property(e => e.CATATAN_9)
                .IsUnicode(false);

            modelBuilder.Entity<POT02E>()
                .Property(e => e.CATATAN_10)
                .IsUnicode(false);

            modelBuilder.Entity<POT02E>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT02F>()
                .Property(e => e.POP_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT02F>()
                .Property(e => e.NO_SO)
                .IsUnicode(false);

            modelBuilder.Entity<POT02F>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT03>()
                .Property(e => e.NO_MATRIK)
                .IsUnicode(false);

            modelBuilder.Entity<POT03>()
                .Property(e => e.POP_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT03>()
                .Property(e => e.ITEM_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT03>()
                .Property(e => e.ITEM_DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<POT03>()
                .Property(e => e.VALUTA)
                .IsUnicode(false);

            modelBuilder.Entity<POT03>()
                .Property(e => e.KODE_SUPPLIER1)
                .IsUnicode(false);

            modelBuilder.Entity<POT03>()
                .Property(e => e.KODE_SUPPLIER2)
                .IsUnicode(false);

            modelBuilder.Entity<POT03>()
                .Property(e => e.KODE_SUPPLIER3)
                .IsUnicode(false);

            modelBuilder.Entity<POT03>()
                .Property(e => e.KODE_SUPPLIER4)
                .IsUnicode(false);

            modelBuilder.Entity<POT03>()
                .Property(e => e.NAMA_SUPPLIER1)
                .IsUnicode(false);

            modelBuilder.Entity<POT03>()
                .Property(e => e.NAMA_SUPPLIER2)
                .IsUnicode(false);

            modelBuilder.Entity<POT03>()
                .Property(e => e.NAMA_SUPPLIER3)
                .IsUnicode(false);

            modelBuilder.Entity<POT03>()
                .Property(e => e.NAMA_SUPPLIER4)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.MATRIX)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.BUYER_CODE)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.POP_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.SUPP1)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.SUPP2)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.SUPP3)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.APPROVE_OLEH)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.SUPP_TERPILIH)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.PO)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.ALAMAT)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.ALASAN)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.SUPP4)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.SUPP5)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.SUPP6)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.PO_NO_1)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.PO_NO_2)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.PO_NO_3)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.PO_NO_4)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.PO_NO_5)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.PO_NO_6)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT03A>()
                .HasMany(e => e.POT03C)
                .WithRequired(e => e.POT03A)
                .HasForeignKey(e => e.NO_MATRIK)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.MATRIX)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.BK)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.SUPP4)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.NAMA2)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.NAMA3)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.CATATAN1)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.CATATAN2)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.CATATAN3)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.NAMA4)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.NAMA5)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.NAMA6)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.CATATAN4)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.CATATAN5)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.CATATAN6)
                .IsUnicode(false);

            modelBuilder.Entity<POT03B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT03C>()
                .Property(e => e.NO_MATRIK)
                .IsUnicode(false);

            modelBuilder.Entity<POT03C>()
                .Property(e => e.SUPPLIER_CODE)
                .IsUnicode(false);

            modelBuilder.Entity<POT03C>()
                .Property(e => e.CATATAN_1)
                .IsUnicode(false);

            modelBuilder.Entity<POT03C>()
                .Property(e => e.CATATAN_2)
                .IsUnicode(false);

            modelBuilder.Entity<POT03C>()
                .Property(e => e.CATATAN_3)
                .IsUnicode(false);

            modelBuilder.Entity<POT03C>()
                .Property(e => e.CATATAN_4)
                .IsUnicode(false);

            modelBuilder.Entity<POT03C>()
                .Property(e => e.CATATAN_5)
                .IsUnicode(false);

            modelBuilder.Entity<POT03C>()
                .Property(e => e.CATATAN_6)
                .IsUnicode(false);

            modelBuilder.Entity<POT03C>()
                .Property(e => e.CATATAN_7)
                .IsUnicode(false);

            modelBuilder.Entity<POT03C>()
                .Property(e => e.CATATAN_8)
                .IsUnicode(false);

            modelBuilder.Entity<POT03C>()
                .Property(e => e.CATATAN_9)
                .IsUnicode(false);

            modelBuilder.Entity<POT03C>()
                .Property(e => e.CATATAN_10)
                .IsUnicode(false);

            modelBuilder.Entity<POT03C>()
                .Property(e => e.KETERANGAN)
                .IsUnicode(false);

            modelBuilder.Entity<POT03C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT03D>()
                .Property(e => e.Matrix)
                .IsUnicode(false);

            modelBuilder.Entity<POT03D>()
                .Property(e => e.Brg)
                .IsUnicode(false);

            modelBuilder.Entity<POT03D>()
                .Property(e => e.Merek)
                .IsUnicode(false);

            modelBuilder.Entity<POT03D>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT04A>()
                .Property(e => e.PO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT04A>()
                .Property(e => e.CATATAN)
                .IsUnicode(false);

            modelBuilder.Entity<POT04A>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<POT04A>()
                .Property(e => e.JENIS_HITUNG)
                .IsUnicode(false);

            modelBuilder.Entity<POT04A>()
                .Property(e => e.Catatan2)
                .IsUnicode(false);

            modelBuilder.Entity<POT04A>()
                .Property(e => e.Catatan3)
                .IsUnicode(false);

            modelBuilder.Entity<POT04A>()
                .Property(e => e.Catatan4)
                .IsUnicode(false);

            modelBuilder.Entity<POT04A>()
                .Property(e => e.Catatan5)
                .IsUnicode(false);

            modelBuilder.Entity<POT04A>()
                .Property(e => e.Catatan6)
                .IsUnicode(false);

            modelBuilder.Entity<POT04A>()
                .Property(e => e.Catatan7)
                .IsUnicode(false);

            modelBuilder.Entity<POT04A>()
                .Property(e => e.NO_LPB)
                .IsUnicode(false);

            modelBuilder.Entity<POT04A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT04B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POT04B>()
                .Property(e => e.PO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT04B>()
                .Property(e => e.PO_Brg)
                .IsUnicode(false);

            modelBuilder.Entity<POT04B>()
                .Property(e => e.Unit_Code)
                .IsUnicode(false);

            modelBuilder.Entity<POT04B>()
                .Property(e => e.NO_PB)
                .IsUnicode(false);

            modelBuilder.Entity<POT04B>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<POT04B>()
                .Property(e => e.Valuta)
                .IsUnicode(false);

            modelBuilder.Entity<POT04B>()
                .Property(e => e.REF_LPB)
                .IsUnicode(false);

            modelBuilder.Entity<POT04B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT04C>()
                .Property(e => e.PO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT04C>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POT04C>()
                .Property(e => e.BIAYA)
                .IsUnicode(false);

            modelBuilder.Entity<POT04C>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<POT04C>()
                .Property(e => e.PO_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POT04C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT04D>()
                .Property(e => e.PO_NO)
                .IsUnicode(false);

            modelBuilder.Entity<POT04D>()
                .Property(e => e.BIAYA)
                .IsUnicode(false);

            modelBuilder.Entity<POT04D>()
                .Property(e => e.CATATAN)
                .IsUnicode(false);

            modelBuilder.Entity<POT04D>()
                .Property(e => e.JENIS_HITUNG)
                .IsUnicode(false);

            modelBuilder.Entity<POT04D>()
                .Property(e => e.CATATAN2)
                .IsUnicode(false);

            modelBuilder.Entity<POT04D>()
                .Property(e => e.CATATAN3)
                .IsUnicode(false);

            modelBuilder.Entity<POT04D>()
                .Property(e => e.CATATAN4)
                .IsUnicode(false);

            modelBuilder.Entity<POT04D>()
                .Property(e => e.CATATAN5)
                .IsUnicode(false);

            modelBuilder.Entity<POT04D>()
                .Property(e => e.CATATAN6)
                .IsUnicode(false);

            modelBuilder.Entity<POT04D>()
                .Property(e => e.CATATAN7)
                .IsUnicode(false);

            modelBuilder.Entity<POT04D>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT05A>()
                .Property(e => e.HEADER)
                .IsUnicode(false);

            modelBuilder.Entity<POT05A>()
                .Property(e => e.FOOTER)
                .IsUnicode(false);

            modelBuilder.Entity<POT05A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT05B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT05C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<POT05D>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SDF08>()
                .Property(e => e.SLM)
                .IsUnicode(false);

            modelBuilder.Entity<SDF08>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<SIF01>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF01>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<SIF01>()
                .Property(e => e.AL)
                .IsUnicode(false);

            modelBuilder.Entity<SIF01>()
                .Property(e => e.AL1)
                .IsUnicode(false);

            modelBuilder.Entity<SIF01>()
                .Property(e => e.AL2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF01>()
                .Property(e => e.AL3)
                .IsUnicode(false);

            modelBuilder.Entity<SIF01>()
                .Property(e => e.WIL)
                .IsUnicode(false);

            modelBuilder.Entity<SIF01>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF02>()
                .Property(e => e.TYPE)
                .IsUnicode(false);

            modelBuilder.Entity<SIF02>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<SIF02>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF03>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<SIF03>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF04>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF04>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.NAMA_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.AL)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.AL1)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.AL2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.AL3)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.CUST_PPN)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.KODE_BUSINESS_LINE)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.CAT_1)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.CAT_2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.CAT_3)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.TBL_HJUAL2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.SET_NOFPAJAK)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.SET_TGLFPAJAK)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.SET_TGLJTTEMPO)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.METODA_HJUAL)
                .IsUnicode(false);

            modelBuilder.Entity<SIF05>()
                .Property(e => e.KODE_PRINCIPAL)
                .IsUnicode(false);

            modelBuilder.Entity<SIF06>()
                .Property(e => e.WIL)
                .IsUnicode(false);

            modelBuilder.Entity<SIF06>()
                .Property(e => e.LEVEL_ITEM)
                .IsUnicode(false);

            modelBuilder.Entity<SIF06>()
                .Property(e => e.KODE_LEVEL)
                .IsUnicode(false);

            modelBuilder.Entity<SIF06>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SIF06>()
                .Property(e => e.SLM)
                .IsUnicode(false);

            modelBuilder.Entity<SIF06>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF06>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF07>()
                .Property(e => e.KDHARGA)
                .IsUnicode(false);

            modelBuilder.Entity<SIF07>()
                .Property(e => e.LEVEL_SORT_FIELD)
                .IsUnicode(false);

            modelBuilder.Entity<SIF07>()
                .Property(e => e.KODE_SORT_FIELD)
                .IsUnicode(false);

            modelBuilder.Entity<SIF07>()
                .Property(e => e.KODE_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SIF07>()
                .Property(e => e.LEVEL_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF07>()
                .Property(e => e.KODE_LEVEL_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF07>()
                .Property(e => e.KODE_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF07>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF08A>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF08A>()
                .Property(e => e.UserName)
                .IsUnicode(false);

            modelBuilder.Entity<SIF08A>()
                .HasMany(e => e.SIF08B)
                .WithRequired(e => e.SIF08A)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SIF08B>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF08B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SIF08B>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<SIF08B>()
                .Property(e => e.BRG_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF08B>()
                .Property(e => e.NAMA_BRG_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF08B>()
                .Property(e => e.UserName)
                .IsUnicode(false);

            modelBuilder.Entity<SIF08B>()
                .Property(e => e.NAMA2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF08B>()
                .Property(e => e.NAMA3)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_1)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_3)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_4)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_5)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_6)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_7)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_8)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_9)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_10)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_11)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_12)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_13)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_14)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_15)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_16)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_17)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_18)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_19)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.CAT_CAPTION_20)
                .IsUnicode(false);

            modelBuilder.Entity<SIF09>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF10>()
                .Property(e => e.KODE_BUSINESS_LINE)
                .IsUnicode(false);

            modelBuilder.Entity<SIF10>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<SIF10>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF11>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SIF11>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.CUST_QQ)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.AL)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.AL2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.AL3)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.TLP)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.PERSON)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.NPWP)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.WIL)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.SLM)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.JKEL)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.AGAMA)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.FAX)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.EMAIL)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.CATAT1)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.CATAT2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF12>()
                .Property(e => e.NAWIL)
                .IsUnicode(false);

            modelBuilder.Entity<SIF13A>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SIF13A>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF13A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF13A>()
                .HasMany(e => e.SIF13B)
                .WithRequired(e => e.SIF13A)
                .HasForeignKey(e => new { e.BRG, e.CUST })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SIF13B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SIF13B>()
                .Property(e => e.WIL)
                .IsUnicode(false);

            modelBuilder.Entity<SIF13B>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF13B>()
                .Property(e => e.CUST_QQ)
                .IsUnicode(false);

            modelBuilder.Entity<SIF13B>()
                .Property(e => e.BK)
                .IsUnicode(false);

            modelBuilder.Entity<SIF13B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF14>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<SIF14>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<SIF14>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<SIF14>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF15A>()
                .Property(e => e.GRUP)
                .IsUnicode(false);

            modelBuilder.Entity<SIF15A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<SIF15A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF15B>()
                .Property(e => e.GRUP)
                .IsUnicode(false);

            modelBuilder.Entity<SIF15B>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF15B>()
                .Property(e => e.Kode_Gudang)
                .IsUnicode(false);

            modelBuilder.Entity<SIF15B>()
                .Property(e => e.KDHARGA)
                .IsUnicode(false);

            modelBuilder.Entity<SIF15B>()
                .Property(e => e.KDDISC)
                .IsUnicode(false);

            modelBuilder.Entity<SIF15B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF16A>()
                .Property(e => e.KDHARGA)
                .IsUnicode(false);

            modelBuilder.Entity<SIF16A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<SIF16A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF16B>()
                .Property(e => e.KDHARGA)
                .IsUnicode(false);

            modelBuilder.Entity<SIF16B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SIF16B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF16C>()
                .Property(e => e.KDHARGA)
                .IsUnicode(false);

            modelBuilder.Entity<SIF16C>()
                .Property(e => e.Sort1)
                .IsUnicode(false);

            modelBuilder.Entity<SIF16C>()
                .Property(e => e.Sort2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF16C>()
                .Property(e => e.Sort3)
                .IsUnicode(false);

            modelBuilder.Entity<SIF16C>()
                .Property(e => e.Sort4)
                .IsUnicode(false);

            modelBuilder.Entity<SIF16C>()
                .Property(e => e.Sort5)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17A>()
                .Property(e => e.GRUP)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17B>()
                .Property(e => e.GRUP)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17B>()
                .Property(e => e.Sort1)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17B>()
                .Property(e => e.Sort2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17B>()
                .Property(e => e.Sort3)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17B>()
                .Property(e => e.Sort4)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17B>()
                .Property(e => e.Sort5)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17B>()
                .Property(e => e.JENIS1)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17B>()
                .Property(e => e.JENIS2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17B>()
                .Property(e => e.Brg)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17B>()
                .Property(e => e.JENIS3)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17B>()
                .Property(e => e.JENIS4)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17B>()
                .Property(e => e.JENIS5)
                .IsUnicode(false);

            modelBuilder.Entity<SIF17B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18A>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18B>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18B>()
                .Property(e => e.Sort1)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18B>()
                .Property(e => e.Sort2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18B>()
                .Property(e => e.Sort3)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18B>()
                .Property(e => e.Sort4)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18B>()
                .Property(e => e.Sort5)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18B>()
                .Property(e => e.JENIS1)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18B>()
                .Property(e => e.JENIS2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18B>()
                .Property(e => e.Brg)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18B>()
                .Property(e => e.JENIS3)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18B>()
                .Property(e => e.JENIS4)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18B>()
                .Property(e => e.JENIS5)
                .IsUnicode(false);

            modelBuilder.Entity<SIF18B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIF22>()
                .Property(e => e.Kd_Kasir)
                .IsUnicode(false);

            modelBuilder.Entity<SIF22>()
                .Property(e => e.Nama)
                .IsUnicode(false);

            modelBuilder.Entity<SIF22>()
                .Property(e => e.AL1)
                .IsUnicode(false);

            modelBuilder.Entity<SIF22>()
                .Property(e => e.AL2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF22>()
                .Property(e => e.TLP)
                .IsUnicode(false);

            modelBuilder.Entity<SIF23>()
                .Property(e => e.KODE_BANK)
                .IsUnicode(false);

            modelBuilder.Entity<SIF23>()
                .Property(e => e.BANK)
                .IsUnicode(false);

            modelBuilder.Entity<SIF23>()
                .Property(e => e.Keterangan)
                .IsUnicode(false);

            modelBuilder.Entity<SIF24>()
                .Property(e => e.KODE_KARTU)
                .IsUnicode(false);

            modelBuilder.Entity<SIF24>()
                .Property(e => e.NAMA_KARTU)
                .IsUnicode(false);

            modelBuilder.Entity<SIFLOCK>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<SIFLOCK>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SIFLOCK>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIFLOCK>()
                .Property(e => e.MACHINE_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.GUDANG)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.METODA_SJ)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.METODA_NO)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.JENIS_SJ)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.PROTEK_QOH)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.PROTEK_LIMIT)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.PROTEK_HARGA)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.NS_SJ)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.NS_FA)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.NS_RT)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.NS_FP)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.METODA_SO)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.NAMA_PT)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.ALAMAT_PT)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.NPWP)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.SK)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.JENIS_PRINT)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.POSTING_STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.LINK_GL)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.JT_SJ)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.JT_FAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.JT_RETUR)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.H_JUAL_TERENDAH)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.KONFIGURASI_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.METODA_LINK)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.NS_Faktur_PPN)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.TINGKAT_DISC)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.METODA_NDISC)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.TERBILANG_IN)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.METODA_HJUAL)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.UPDATE_QOH_SJ)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.TOTAL_DISC_per)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.EDIT_DISC_perItem)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.EDIT_DISC_perFaktur)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.DEFAULT_TBL_HJUAL)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.METODA_LINK_CC)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.VALIDASI_U_MUKA)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.KODE_BRG_STYLE)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.DB)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.JTRAN_RETUR)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.JTRAN_RETUR_KONSINYASI)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.KODE_VALUTA)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.PROTEK_SO)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.GUDANG_SALESMAN)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.BARANG_SAMA)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.EDIT_BONUS)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.VALIDASI_BRG_FAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.METODA_DISCOUNT)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.USERNAME_POSTING)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.TERBILANG)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.MFDesimal)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.QFDesimal)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.DB_PATH)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.DIRECT_SALES)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSY>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSYS_DS>()
                .Property(e => e.Penomoran)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSYS_DS>()
                .Property(e => e.PrefixOrder)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSYS_DS>()
                .Property(e => e.PrefixFaktur)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSYS_DS>()
                .Property(e => e.RekGLCC)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSYS_DS>()
                .Property(e => e.RekGLDC)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSYS_DS>()
                .Property(e => e.RekGLTC)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSYS_DS>()
                .Property(e => e.PrefixPemb)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSYS_DS>()
                .Property(e => e.RekReturAR)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSYS_DS>()
                .Property(e => e.No_Seri_Kas)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSYS_DS>()
                .Property(e => e.No_Seri_GM)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSYS_DS>()
                .Property(e => e.RekGiroMundur)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSYS_DS>()
                .Property(e => e.RekDisc)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSYS_DS>()
                .Property(e => e.RekSelisihBayar)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSYS_DS>()
                .Property(e => e.RekPembyranSementara)
                .IsUnicode(false);

            modelBuilder.Entity<SIFSYS_DS>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.ST_POSTING)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NO_REF)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NO_SO)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NAMA_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.KODE_ALAMAT)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NO_KENDARAAN)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.SOPIR)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.KODE_SALES)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.KODE_WIL)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NO_F_PAJAK)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.KODE_PROYEK)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NO_MK)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.JENIS_RETUR)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.JTRAN)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.AL3)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.AL2)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.AL1)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.AL)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.JENIS)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.CUST_QQ)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NAMA_CUST_QQ)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NO_FAKTUR_PPN_AR)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NO_FAKTUR_LAMA)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.STATUS_LOADING)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NO_FA_OUTLET)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NO_LPB)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NO_PO_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.GROUP_LIMIT)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.KODE_ANGKUTAN)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.JENIS_MOBIL)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.PENGIRIM)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NAMAPENGIRIM)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.ZONA)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.UCAPAN)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.PEMESAN)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NAMAPEMESAN)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.NO_SERI_VOUCHER)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.USERNAME_POSTING)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .Property(e => e.USERNAME_APPROVAL)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01A>()
                .HasMany(e => e.SIT01B)
                .WithRequired(e => e.SIT01A)
                .HasForeignKey(e => new { e.JENIS_FORM, e.NO_BUKTI })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SIT01A>()
                .HasMany(e => e.SIT01B1)
                .WithRequired(e => e.SIT01A)
                .HasForeignKey(e => new { e.JENIS_FORM, e.NO_BUKTI })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SIT01A>()
                .HasMany(e => e.SIT01C)
                .WithRequired(e => e.SIT01A)
                .HasForeignKey(e => new { e.JENIS_FORM, e.NO_BUKTI })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SIT01A>()
                .HasOptional(e => e.SIT01F)
                .WithRequired(e => e.SIT01A);

            modelBuilder.Entity<SIT01B>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B>()
                .Property(e => e.BRG_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B>()
                .Property(e => e.GUDANG)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B>()
                .Property(e => e.AUTO_LOAD)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B>()
                .Property(e => e.CATATAN)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B>()
                .Property(e => e.BRG_SO)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B1>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B1>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B1>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B1>()
                .Property(e => e.JENIS_SIZE)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B1>()
                .Property(e => e.GUDANG)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B1>()
                .Property(e => e.AUTO_LOAD)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B1>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B1>()
                .Property(e => e.CATATAN)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B1>()
                .Property(e => e.SORT1)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B1>()
                .Property(e => e.SORT2)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B1>()
                .Property(e => e.SORT3)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01B1>()
                .Property(e => e.SORT4)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01C>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01C>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01C>()
                .Property(e => e.NO_SJ)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01D>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01D>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01D>()
                .Property(e => e.KODE_BRG_UNIT)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01D>()
                .Property(e => e.KODE_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01D>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01D>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01D>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01E>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01E>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01E>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01E>()
                .Property(e => e.LOT_NO)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01E>()
                .Property(e => e.BATCH_NO)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01E>()
                .Property(e => e.GUD)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01E>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01E>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01E>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01E>()
                .Property(e => e.SPESIFIKASI)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01F>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01F>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01F>()
                .Property(e => e.CATATAN_1)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01F>()
                .Property(e => e.CATATAN_2)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01F>()
                .Property(e => e.CATATAN_3)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01F>()
                .Property(e => e.CATATAN_4)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01F>()
                .Property(e => e.CATATAN_5)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01F>()
                .Property(e => e.CATATAN_6)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01F>()
                .Property(e => e.CATATAN_7)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01F>()
                .Property(e => e.CATATAN_8)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01F>()
                .Property(e => e.CATATAN_9)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01F>()
                .Property(e => e.CATATAN_10)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01F>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01G>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01G>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01G>()
                .Property(e => e.NO_FAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<SIT01G>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIT02A>()
                .Property(e => e.KD_KASIR)
                .IsUnicode(false);

            modelBuilder.Entity<SIT02A>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<SIT02A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIT02B>()
                .Property(e => e.KD_KASIR)
                .IsUnicode(false);

            modelBuilder.Entity<SIT02B>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<SIT02B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIT02C>()
                .Property(e => e.KD_KASIR)
                .IsUnicode(false);

            modelBuilder.Entity<SIT02C>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<SIT02C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03A>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03A>()
                .Property(e => e.KD_KASIR)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03A>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03A>()
                .Property(e => e.NAMA_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03A>()
                .Property(e => e.STATUS_BAYAR)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03A>()
                .Property(e => e.NOMOR_CEK)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03A>()
                .Property(e => e.BANK_CEK)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03A>()
                .Property(e => e.NOMOR_DEBET)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03A>()
                .Property(e => e.BANK_DEBET)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03A>()
                .Property(e => e.ST_POSTING)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03A>()
                .Property(e => e.NoRetur)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03A>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03A>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03B>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03B>()
                .Property(e => e.NO_FAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03B>()
                .Property(e => e.KD_KASIR)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03C>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03C>()
                .Property(e => e.KODE_KARTU)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03C>()
                .Property(e => e.NO_KARTU_KREDIT)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03C>()
                .Property(e => e.Bank)
                .IsUnicode(false);

            modelBuilder.Entity<SIT03C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_1)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_2)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_3)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_4)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_5)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_6)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_7)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_8)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_9)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_10)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_11)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_12)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_13)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_14)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_15)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_16)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_17)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_18)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_19)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.CAT_CAPTION_20)
                .IsUnicode(false);

            modelBuilder.Entity<SOF01>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SOF02>()
                .Property(e => e.UserName)
                .IsUnicode(false);

            modelBuilder.Entity<SOF02>()
                .Property(e => e.Vlt)
                .IsUnicode(false);

            modelBuilder.Entity<SOF03>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SOFLOCK>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SOFLOCK>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<SOFLOCK>()
                .Property(e => e.MACHINE_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.METODA_NO)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.NS_SO)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.PROTEK_QOH)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.PROTEK_LIMIT)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.PROTEK_HSATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.H_JUAL_TERENDAH)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.KONFIGURASI_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.VALIDASI_TOLERANSI_KIRIM)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.NS_PENAWARAN)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.APPROVAL_SO)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.SECURITY_SO)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.MFDesimal)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.QFDesimal)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.DB_PATH)
                .IsUnicode(false);

            modelBuilder.Entity<SOFSY>()
                .Property(e => e.EXPIRED)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.NO_PO_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.NAMA_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.KODE_SALES)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.KODE_WIL)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.KODE_ALAMAT)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.AL)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.AL1)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.AL2)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.AL3)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.AL_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.CUST_QQ)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.HARGA_FRANCO)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.Status_Approve)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.User_Approve)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.NO_PENAWARAN)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.PENGIRIM)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.NAMAPENGIRIM)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.ZONA)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.UCAPAN)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.PEMESAN)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.NAMAPEMESAN)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.EXPEDISI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.STATUS_TRANSAKSI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.ALAMAT_KIRIM)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.PROPINSI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.KOTA)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.KODE_POS)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.SHIPMENT)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .Property(e => e.TRACKING_SHIPMENT)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01A>()
                .HasOptional(e => e.SOT01D)
                .WithRequired(e => e.SOT01A);

            modelBuilder.Entity<SOT01B>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01B>()
                .Property(e => e.BRG_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01B>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01B>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01B>()
                .Property(e => e.LOKASI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01B>()
                .Property(e => e.CATATAN)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01B2>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01B2>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01B2>()
                .Property(e => e.JENIS_SIZE)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01B2>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01B2>()
                .Property(e => e.LOKASI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01B2>()
                .Property(e => e.SORT1)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01B2>()
                .Property(e => e.SORT2)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01B2>()
                .Property(e => e.SORT3)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01B2>()
                .Property(e => e.SORT4)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01C>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01C>()
                .Property(e => e.KODE_BRG_UNIT)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01C>()
                .Property(e => e.KODE_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01C>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01D>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01D>()
                .Property(e => e.CATATAN_1)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01D>()
                .Property(e => e.CATATAN_2)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01D>()
                .Property(e => e.CATATAN_3)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01D>()
                .Property(e => e.CATATAN_4)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01D>()
                .Property(e => e.CATATAN_5)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01D>()
                .Property(e => e.CATATAN_6)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01D>()
                .Property(e => e.CATATAN_7)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01D>()
                .Property(e => e.CATATAN_8)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01D>()
                .Property(e => e.CATATAN_9)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01D>()
                .Property(e => e.CATATAN_10)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01D>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01E>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01E>()
                .Property(e => e.GUDANG)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01E>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01E>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01F>()
                .Property(e => e.No_Bukti)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01F>()
                .Property(e => e.Brg)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01F>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<SOT01F>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.NOBUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.NAMA_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.KODE_SALES)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.KODE_WIL)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.KODE_ALAMAT)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.AL)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.AL1)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.AL2)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.AL3)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.AL_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.CUST_QQ)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.HARGA_FRANCO)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02A>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02B>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02B>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02B>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02B>()
                .Property(e => e.LOKASI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02B>()
                .Property(e => e.CATATAN)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02B>()
                .Property(e => e.BRG_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02C>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02C>()
                .Property(e => e.KODE_BRG_UNIT)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02C>()
                .Property(e => e.KODE_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02C>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02D>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02D>()
                .Property(e => e.CATATAN_1)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02D>()
                .Property(e => e.CATATAN_2)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02D>()
                .Property(e => e.CATATAN_3)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02D>()
                .Property(e => e.CATATAN_4)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02D>()
                .Property(e => e.CATATAN_5)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02D>()
                .Property(e => e.CATATAN_6)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02D>()
                .Property(e => e.CATATAN_7)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02D>()
                .Property(e => e.CATATAN_8)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02D>()
                .Property(e => e.CATATAN_9)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02D>()
                .Property(e => e.CATATAN_10)
                .IsUnicode(false);

            modelBuilder.Entity<SOT02D>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.NAMA2)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.NAMA3)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.JENIS)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.STN2)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.MVC)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.LKS)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.SUP)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.MEREK)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.PART)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.TYPE)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.KLINK)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.WARNA)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Sort1)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Sort2)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Sort3)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Sort4)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Sort5)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Stn_berat)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Metoda)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Qty_berat)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.KET_SORT1)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.KET_SORT2)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.KET_SORT3)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.KET_SORT4)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.KET_SORT5)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.STN3)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.STN4)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.KET_STN2)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.KET_STN3)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.KET_STN4)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.KET_STN)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Sort6)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Sort7)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Sort8)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Sort9)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Sort10)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Ket_Sort6)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Ket_Sort7)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Ket_Sort8)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Ket_Sort9)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.Ket_Sort10)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.BSK)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.BRG_NON_OS)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .Property(e => e.PHOTO2)
                .IsUnicode(false);

            modelBuilder.Entity<STF02>()
                .HasMany(e => e.STF02B)
                .WithRequired(e => e.STF02)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<STF02>()
                .HasOptional(e => e.STF02C)
                .WithRequired(e => e.STF02);

            modelBuilder.Entity<STF02>()
                .HasMany(e => e.STF02D)
                .WithRequired(e => e.STF02)
                .HasForeignKey(e => e.UNIT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<STF02>()
                .HasOptional(e => e.STF02F)
                .WithRequired(e => e.STF02);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.BRG1)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.BRG2)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.BRG3)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.BRG4)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.BRG5)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.BRG6)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.BRG7)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.BRG8)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.BRG9)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.BRG10)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.BRG11)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.BRG12)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.BRG13)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.BRG14)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.BRG15)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.NAMA2)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.NAMA3)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.JENIS_SIZE)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.JENIS)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.STN2)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.MVC)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.LKS)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.SUP)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.MEREK)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.PART1)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.PART2)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.PART3)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.PART4)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.PART5)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.PART6)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.PART7)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.PART8)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.PART9)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.PART10)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.PART11)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.PART12)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.PART13)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.PART14)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.PART15)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.TYPE)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.KLINK)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.WARNA)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.Sort1)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.Sort2)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.Sort3)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.Sort4)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.Stn_berat)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.Metoda)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.Qty_berat)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.KET_SORT1)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.KET_SORT2)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.KET_SORT3)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.KET_SORT4)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.STN3)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.STN4)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.KET_STN2)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.KET_STN3)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.KET_STN4)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.KET_STN)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.SORT5)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.SORT6)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.SORT7)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.SORT8)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.SORT9)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.SORT10)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.KET_SORT5)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.KET_SORT6)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.KET_SORT7)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.KET_SORT8)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.KET_SORT9)
                .IsUnicode(false);

            modelBuilder.Entity<STF02A1>()
                .Property(e => e.KET_SORT10)
                .IsUnicode(false);

            modelBuilder.Entity<STF02B>()
                .Property(e => e.Brg)
                .IsUnicode(false);

            modelBuilder.Entity<STF02B>()
                .Property(e => e.KDHarga)
                .IsUnicode(false);

            modelBuilder.Entity<STF02B>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<STF02B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STF02B>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<STF02C>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF02C>()
                .Property(e => e.STN_3)
                .IsUnicode(false);

            modelBuilder.Entity<STF02C>()
                .Property(e => e.STN_4)
                .IsUnicode(false);

            modelBuilder.Entity<STF02D>()
                .Property(e => e.UNIT)
                .IsUnicode(false);

            modelBuilder.Entity<STF02D>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF02D>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<STF02D>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<STF02D>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STF02E>()
                .Property(e => e.LEVEL)
                .IsUnicode(false);

            modelBuilder.Entity<STF02E>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<STF02E>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<STF02E>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_1)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_2)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_3)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_4)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_5)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_6)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_7)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_8)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_9)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_10)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_11)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_12)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_13)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_14)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_15)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_16)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_17)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_18)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_19)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.SPESIFIKASI_20)
                .IsUnicode(false);

            modelBuilder.Entity<STF02F>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STF03>()
                .Property(e => e.Unit)
                .IsUnicode(false);

            modelBuilder.Entity<STF03>()
                .Property(e => e.Brg)
                .IsUnicode(false);

            modelBuilder.Entity<STF03>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STF03>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STF04>()
                .Property(e => e.TYPE)
                .IsUnicode(false);

            modelBuilder.Entity<STF04>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STF04>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF05>()
                .Property(e => e.Gd)
                .IsUnicode(false);

            modelBuilder.Entity<STF05>()
                .Property(e => e.Brg)
                .IsUnicode(false);

            modelBuilder.Entity<STF05A>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STF05A>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF06>()
                .Property(e => e.LKS)
                .IsUnicode(false);

            modelBuilder.Entity<STF06>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<STF07>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STF07>()
                .Property(e => e.GUDANG)
                .IsUnicode(false);

            modelBuilder.Entity<STF07>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<STF08>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STF08>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF08A>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STF08A>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF08B>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STF08B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF08B>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<STF09>()
                .Property(e => e.Brg)
                .IsUnicode(false);

            modelBuilder.Entity<STF09>()
                .Property(e => e.Bukti)
                .IsUnicode(false);

            modelBuilder.Entity<STF09>()
                .Property(e => e.MK)
                .IsUnicode(false);

            modelBuilder.Entity<STF09>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<STF09>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STF09>()
                .Property(e => e.Ref)
                .IsUnicode(false);

            modelBuilder.Entity<STF09>()
                .Property(e => e.JTran)
                .IsUnicode(false);

            modelBuilder.Entity<STF09>()
                .Property(e => e.WO)
                .IsUnicode(false);

            modelBuilder.Entity<STF09>()
                .Property(e => e.No_Faktur)
                .IsUnicode(false);

            modelBuilder.Entity<STF09>()
                .Property(e => e.BRG_UNIT)
                .IsUnicode(false);

            modelBuilder.Entity<STF09>()
                .Property(e => e.Work_Center)
                .IsUnicode(false);

            modelBuilder.Entity<STF09A>()
                .Property(e => e.Brg)
                .IsUnicode(false);

            modelBuilder.Entity<STF09A>()
                .Property(e => e.Bukti)
                .IsUnicode(false);

            modelBuilder.Entity<STF09A>()
                .Property(e => e.MK)
                .IsUnicode(false);

            modelBuilder.Entity<STF09A>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<STF09A>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STF09A>()
                .Property(e => e.Ref)
                .IsUnicode(false);

            modelBuilder.Entity<STF09A>()
                .Property(e => e.JTran)
                .IsUnicode(false);

            modelBuilder.Entity<STF09A>()
                .Property(e => e.WO)
                .IsUnicode(false);

            modelBuilder.Entity<STF09A>()
                .Property(e => e.No_Faktur)
                .IsUnicode(false);

            modelBuilder.Entity<STF09A>()
                .Property(e => e.Work_Center)
                .IsUnicode(false);

            modelBuilder.Entity<STF09A>()
                .Property(e => e.DR_GD)
                .IsUnicode(false);

            modelBuilder.Entity<STF09A>()
                .Property(e => e.BRG_UNIT)
                .IsUnicode(false);

            modelBuilder.Entity<STF09B>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STF09B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF09B>()
                .Property(e => e.BUKTI_MSK)
                .IsUnicode(false);

            modelBuilder.Entity<STF09C>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STF09C>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF09C>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<STF09C>()
                .Property(e => e.BUKTI_MSK)
                .IsUnicode(false);

            modelBuilder.Entity<STF09C>()
                .Property(e => e.MK)
                .IsUnicode(false);

            modelBuilder.Entity<STF09C>()
                .Property(e => e.WO)
                .IsUnicode(false);

            modelBuilder.Entity<STF10>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF10>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STF10>()
                .Property(e => e._ref)
                .IsUnicode(false);

            modelBuilder.Entity<STF10>()
                .Property(e => e.LOT_NO)
                .IsUnicode(false);

            modelBuilder.Entity<STF10>()
                .Property(e => e.BATCH_NO)
                .IsUnicode(false);

            modelBuilder.Entity<STF10B>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STF10B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF10B>()
                .Property(e => e.WO)
                .IsUnicode(false);

            modelBuilder.Entity<STF11>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF11B>()
                .Property(e => e.MPS)
                .IsUnicode(false);

            modelBuilder.Entity<STF11B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF11C>()
                .Property(e => e.MPS)
                .IsUnicode(false);

            modelBuilder.Entity<STF11C>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF11C>()
                .Property(e => e.BOM)
                .IsUnicode(false);

            modelBuilder.Entity<STF11C>()
                .Property(e => e.BRG_JADI)
                .IsUnicode(false);

            modelBuilder.Entity<STF11D>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.JUD1)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.JUD2)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.JUD3)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.JUD4)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.JUD5)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.JUD6)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.JUD7)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.JUD8)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.JUD9)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.JUD10)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.TRAN1)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.TRAN2)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.TRAN3)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.TRAN4)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.TRAN5)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.TRAN6)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.TRAN7)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.TRAN8)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.TRAN9)
                .IsUnicode(false);

            modelBuilder.Entity<STF12>()
                .Property(e => e.TRAN10)
                .IsUnicode(false);

            modelBuilder.Entity<STF13>()
                .Property(e => e.Kode)
                .IsUnicode(false);

            modelBuilder.Entity<STF13>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<STF13>()
                .Property(e => e.MK)
                .IsUnicode(false);

            modelBuilder.Entity<STF13>()
                .Property(e => e.NO_SERI)
                .IsUnicode(false);

            modelBuilder.Entity<STF13>()
                .Property(e => e.JRef)
                .IsUnicode(false);

            modelBuilder.Entity<STF13>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STF14>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STF16>()
                .Property(e => e.Jenis)
                .IsUnicode(false);

            modelBuilder.Entity<STF16>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<STF16>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STF18>()
                .Property(e => e.Kode_Gudang)
                .IsUnicode(false);

            modelBuilder.Entity<STF18>()
                .Property(e => e.Nama_Gudang)
                .IsUnicode(false);

            modelBuilder.Entity<STF18>()
                .Property(e => e.AL1)
                .IsUnicode(false);

            modelBuilder.Entity<STF18>()
                .Property(e => e.AL2)
                .IsUnicode(false);

            modelBuilder.Entity<STF18>()
                .Property(e => e.AL3)
                .IsUnicode(false);

            modelBuilder.Entity<STF18>()
                .Property(e => e.CUSTOMER)
                .IsUnicode(false);

            modelBuilder.Entity<STF18>()
                .Property(e => e.KD_HARGA_JUAL)
                .IsUnicode(false);

            modelBuilder.Entity<STF18>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STF19>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STF19>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<STFCAT>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<STFCAT>()
                .Property(e => e.CAP_CATATAN_1)
                .IsUnicode(false);

            modelBuilder.Entity<STFCAT>()
                .Property(e => e.CAP_CATATAN_2)
                .IsUnicode(false);

            modelBuilder.Entity<STFCAT>()
                .Property(e => e.CAP_CATATAN_3)
                .IsUnicode(false);

            modelBuilder.Entity<STFCAT>()
                .Property(e => e.CAP_CATATAN_4)
                .IsUnicode(false);

            modelBuilder.Entity<STFCAT>()
                .Property(e => e.CAP_CATATAN_5)
                .IsUnicode(false);

            modelBuilder.Entity<STFCAT>()
                .Property(e => e.CAP_CATATAN_6)
                .IsUnicode(false);

            modelBuilder.Entity<STFCAT>()
                .Property(e => e.CAP_CATATAN_7)
                .IsUnicode(false);

            modelBuilder.Entity<STFCAT>()
                .Property(e => e.CAP_CATATAN_8)
                .IsUnicode(false);

            modelBuilder.Entity<STFCAT>()
                .Property(e => e.CAP_CATATAN_9)
                .IsUnicode(false);

            modelBuilder.Entity<STFCAT>()
                .Property(e => e.CAP_CATATAN_10)
                .IsUnicode(false);

            modelBuilder.Entity<STFLINE>()
                .Property(e => e.KLINE)
                .IsUnicode(false);

            modelBuilder.Entity<STFLOCK01>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<STFLOCK01>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<STFLOCK01>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<STFLOCK01>()
                .Property(e => e.MACHINE_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<STFLOCK02>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<STFLOCK02>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<STFLOCK02>()
                .Property(e => e.USER_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<STFLOCK02>()
                .Property(e => e.MACHINE_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.Gudang)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.Satuan)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.Metoda)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.LSort1)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.LSort2)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.LSort3)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.LSort4)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.LSort5)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ST_K1)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ST_K2)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ST_K3)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ST_K4)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ST_K5)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ST_P1)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ST_P2)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ST_P3)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ST_P4)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ST_P5)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ST_PJ1)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ST_PJ2)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ST_PJ3)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ST_PJ4)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ST_PJ5)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.Stn_Berat)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.LINK_GL)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.POST_STOCK)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.STOCK_MINUS)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.PROMPT_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.POST_KONFIG)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.MFDesimal)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.QFDesimal)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_1)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_2)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_3)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_4)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_5)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_6)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_7)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_8)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_9)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_10)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_11)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_12)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_13)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_14)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_15)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_16)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_17)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_18)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_19)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SPEC_CAPTION_20)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.UPDATE_QOH_SJ)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.METODA_NO)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.NO_SERI_AD_GD)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.NO_SERI_PD)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.NO_SERI_PB)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.ADA_PB)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.POSTING_STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SECURITY_GUDANG)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SLIPTEMPTABLE)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.AUTOLOAD_QTY)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.POST_RETUR)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.METODA_TRANS_PROD)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.Entry_Style)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.LSort6)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.LSort7)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.LSort8)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.LSort9)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.LSort10)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.GD_QC)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.Validasi_Tukar)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SERI_PECAH_BRG_MSK)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SERI_PECAH_BRG_KLR)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SERI_KONSI_BRG_KLR)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.SERI_KONSI_BRG_RETUR)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.DB_PATH)
                .IsUnicode(false);

            modelBuilder.Entity<STFSY>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.KLink)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.Jurnal)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.Sedia)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.HPP)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.WIP)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.Koreksi_D)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.Koreksi_K)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.PJL)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.Retur)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.Beli)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.CC)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.DISC_JUAL)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.DISC_BELI)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.BIAYA_QTY)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.COGM)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.FOH)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.DISC_JUAL_1)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.DISC_JUAL_2)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.DISC_JUAL_3)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.DISC_JUAL_4)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.DISC_JUAL_5)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.PPNBM)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.GIT_SALES)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.SUSUT)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK2A>()
                .Property(e => e.KLINK)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK2A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK2A>()
                .Property(e => e.LKS)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK2A>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK2A>()
                .Property(e => e.COST)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK2A>()
                .Property(e => e.JURNAL)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK2A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK2B>()
                .Property(e => e.KLINK)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK2B>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK2B>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK2B>()
                .Property(e => e.MK)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK2B>()
                .Property(e => e.DEBET)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK2B>()
                .Property(e => e.KREDIT)
                .IsUnicode(false);

            modelBuilder.Entity<STLINK2B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.Nobuk)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.Satuan)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.ST_Cetak)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.ST_Posting)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.JTran)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.MK)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.JRef)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.Ref)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.UserName)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.VALUTA)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.WORK_CENTER)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.STATUS_LOADING)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.KLINE)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.KODE_ANGKUTAN)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.JENIS_MOBIL)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.NO_POLISI)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.NAMA_SOPIR)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.No_PP)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.CATATAN_1)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.CATATAN_2)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.CATATAN_3)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.CATATAN_4)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.CATATAN_5)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.CATATAN_6)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.CATATAN_7)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.CATATAN_8)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.CATATAN_9)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.CATATAN_10)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.NOBUK_POQC)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.Supp)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.NAMA_SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.NO_PL)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .Property(e => e.NO_FAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<STT01A>()
                .HasMany(e => e.STT01B)
                .WithRequired(e => e.STT01A)
                .HasForeignKey(e => new { e.Jenis_Form, e.Nobuk })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<STT01A>()
                .HasMany(e => e.STT01B1)
                .WithRequired(e => e.STT01A)
                .HasForeignKey(e => new { e.Jenis_Form, e.Nobuk })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<STT01B>()
                .Property(e => e.Nobuk)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B>()
                .Property(e => e.Kobar)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B>()
                .Property(e => e.Satuan)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B>()
                .Property(e => e.Ke_Gd)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B>()
                .Property(e => e.Dr_Gd)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B>()
                .Property(e => e.WO)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B>()
                .Property(e => e.Rak)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B>()
                .Property(e => e.JTran)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B>()
                .Property(e => e.UserName)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B>()
                .Property(e => e.KLINK)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B>()
                .Property(e => e.NO_WO)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B>()
                .Property(e => e.BRG_ORIGINAL)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B>()
                .Property(e => e.BUKTI_DS)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B>()
                .Property(e => e.BUKTI_REFF)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B1>()
                .Property(e => e.Nobuk)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B1>()
                .Property(e => e.Satuan)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B1>()
                .Property(e => e.Jenis_Size)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B1>()
                .Property(e => e.Ke_Gd)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B1>()
                .Property(e => e.Dr_Gd)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B1>()
                .Property(e => e.WO)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B1>()
                .Property(e => e.Rak)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B1>()
                .Property(e => e.JTran)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B1>()
                .Property(e => e.UserName)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B1>()
                .Property(e => e.KLINK)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B1>()
                .Property(e => e.NO_WO)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B1>()
                .Property(e => e.SORT1)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B1>()
                .Property(e => e.SORT2)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B1>()
                .Property(e => e.SORT3)
                .IsUnicode(false);

            modelBuilder.Entity<STT01B1>()
                .Property(e => e.SORT4)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C>()
                .Property(e => e.LOT_NO)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C>()
                .Property(e => e.BATCH_NO)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C>()
                .Property(e => e.GUD)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C>()
                .Property(e => e.SPESIFIKASI)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C1>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C1>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C1>()
                .Property(e => e.LOT_NO)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C1>()
                .Property(e => e.BATCH_NO)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C1>()
                .Property(e => e.GUD)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C1>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C1>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C1>()
                .Property(e => e.SPESIFIKASI)
                .IsUnicode(false);

            modelBuilder.Entity<STT01C1>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STT01D>()
                .Property(e => e.Jenis_Form)
                .IsUnicode(false);

            modelBuilder.Entity<STT01D>()
                .Property(e => e.Nobuk)
                .IsUnicode(false);

            modelBuilder.Entity<STT01D>()
                .Property(e => e.KODE_BRG_UNIT)
                .IsUnicode(false);

            modelBuilder.Entity<STT01D>()
                .Property(e => e.KODE_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STT01D>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<STT01D>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STT01D>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STT02>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STT02>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STT02A>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<STT02A>()
                .Property(e => e.JREF)
                .IsUnicode(false);

            modelBuilder.Entity<STT02A>()
                .Property(e => e.REF)
                .IsUnicode(false);

            modelBuilder.Entity<STT02A>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<STT02A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<STT02A>()
                .Property(e => e.WORK_CENTER)
                .IsUnicode(false);

            modelBuilder.Entity<STT02A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STT02A>()
                .HasMany(e => e.STT02B)
                .WithRequired(e => e.STT02A)
                .HasForeignKey(e => new { e.JENIS_FORM, e.NOBUK })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<STT02B>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<STT02B>()
                .Property(e => e.KOBAR)
                .IsUnicode(false);

            modelBuilder.Entity<STT02B>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<STT02B>()
                .Property(e => e.NO_WO)
                .IsUnicode(false);

            modelBuilder.Entity<STT02B>()
                .Property(e => e.CATATAN)
                .IsUnicode(false);

            modelBuilder.Entity<STT02B>()
                .Property(e => e.DR_GD)
                .IsUnicode(false);

            modelBuilder.Entity<STT02B>()
                .Property(e => e.KE_GD)
                .IsUnicode(false);

            modelBuilder.Entity<STT02B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STT03A>()
                .Property(e => e.Nobuk)
                .IsUnicode(false);

            modelBuilder.Entity<STT03A>()
                .Property(e => e.Jenis)
                .IsUnicode(false);

            modelBuilder.Entity<STT03A>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<STT03B>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<STT03B>()
                .Property(e => e.KOBAR)
                .IsUnicode(false);

            modelBuilder.Entity<STT03B>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STT03B>()
                .Property(e => e.Nm_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STT03B>()
                .Property(e => e.Satuan)
                .IsUnicode(false);

            modelBuilder.Entity<STT04A>()
                .Property(e => e.GUD)
                .IsUnicode(false);

            modelBuilder.Entity<STT04A>()
                .Property(e => e.NAMA_GUDANG)
                .IsUnicode(false);

            modelBuilder.Entity<STT04A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STT04A>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<STT04A>()
                .Property(e => e.POSTING)
                .IsUnicode(false);

            modelBuilder.Entity<STT04A>()
                .HasMany(e => e.STT04B)
                .WithRequired(e => e.STT04A)
                .HasForeignKey(e => new { e.NOBUK, e.Gud })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<STT04B>()
                .Property(e => e.Gud)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B>()
                .Property(e => e.Brg)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B>()
                .Property(e => e.BK)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B>()
                .Property(e => e.Stn)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B>()
                .Property(e => e.WO)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B>()
                .Property(e => e.Nama_Barang)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B>()
                .Property(e => e.LKS)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Jenis_Size)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Gud)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Sort1)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Sort2)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Sort3)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Sort4)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Sort5)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.BK_1)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Stn_1)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.WO_1)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.BK_2)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Stn_2)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.WO_2)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.BK_3)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Stn_3)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.WO_3)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.BK_4)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Stn_4)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.WO_4)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.BK_5)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Stn_5)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.WO_5)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.BK_6)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Stn_6)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.WO_6)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.BK_7)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Stn_7)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.WO_7)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.BK_8)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Stn_8)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.WO_8)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.BK_9)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Stn_9)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.WO_9)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.BK_10)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Stn_10)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.WO_10)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.BK_11)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Stn_11)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.WO_11)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.BK_12)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Stn_12)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.WO_12)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.BK_13)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Stn_13)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.WO_13)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.BK_14)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Stn_14)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.WO_14)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.BK_15)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.Stn_15)
                .IsUnicode(false);

            modelBuilder.Entity<STT04B1>()
                .Property(e => e.WO_15)
                .IsUnicode(false);

            modelBuilder.Entity<STT04C>()
                .Property(e => e.Gd)
                .IsUnicode(false);

            modelBuilder.Entity<STT04C>()
                .Property(e => e.Brg)
                .IsUnicode(false);

            modelBuilder.Entity<STT04C>()
                .Property(e => e.Lot_No)
                .IsUnicode(false);

            modelBuilder.Entity<STT04C>()
                .Property(e => e.Batch_No)
                .IsUnicode(false);

            modelBuilder.Entity<STT04C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STT04D>()
                .Property(e => e.GUD)
                .IsUnicode(false);

            modelBuilder.Entity<STT04D>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STT04D>()
                .Property(e => e.BK)
                .IsUnicode(false);

            modelBuilder.Entity<STT04D>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<STT04D>()
                .Property(e => e.WO)
                .IsUnicode(false);

            modelBuilder.Entity<STT04D>()
                .Property(e => e.NABRG)
                .IsUnicode(false);

            modelBuilder.Entity<STT05A>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<STT05A>()
                .Property(e => e.MK)
                .IsUnicode(false);

            modelBuilder.Entity<STT05A>()
                .Property(e => e.DARI)
                .IsUnicode(false);

            modelBuilder.Entity<STT05A>()
                .Property(e => e.KE)
                .IsUnicode(false);

            modelBuilder.Entity<STT05A>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<STT05A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<STT05A>()
                .Property(e => e.JTRAN)
                .IsUnicode(false);

            modelBuilder.Entity<STT05A>()
                .Property(e => e.REF)
                .IsUnicode(false);

            modelBuilder.Entity<STT05A>()
                .Property(e => e.ST_CETAK)
                .IsUnicode(false);

            modelBuilder.Entity<STT05B>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<STT05B>()
                .Property(e => e.KOBAR)
                .IsUnicode(false);

            modelBuilder.Entity<STT05B>()
                .Property(e => e.RAK)
                .IsUnicode(false);

            modelBuilder.Entity<STT05B>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STT05B>()
                .Property(e => e.BS)
                .IsUnicode(false);

            modelBuilder.Entity<STT05B>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<STT05B>()
                .Property(e => e.WO)
                .IsUnicode(false);

            modelBuilder.Entity<STT06>()
                .Property(e => e.Kode_Biaya)
                .IsUnicode(false);

            modelBuilder.Entity<STT06>()
                .Property(e => e.Keterangan)
                .IsUnicode(false);

            modelBuilder.Entity<STT06>()
                .Property(e => e.DEPT)
                .IsUnicode(false);

            modelBuilder.Entity<STT06>()
                .Property(e => e.CC)
                .IsUnicode(false);

            modelBuilder.Entity<STT06>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STT07A>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<STT07B>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<STT07B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STT07B>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<STT07B>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STT07C>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<STT07C>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STT07C>()
                .Property(e => e.LOT_NO)
                .IsUnicode(false);

            modelBuilder.Entity<STT07D>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<STT07D>()
                .Property(e => e.UNIT)
                .IsUnicode(false);

            modelBuilder.Entity<STT07D>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<STT07D>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<STT07D>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<STT07D>()
                .Property(e => e.PLUS_MINUS)
                .IsUnicode(false);

            modelBuilder.Entity<STURUT>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<STURUT>()
                .Property(e => e.Kode)
                .IsUnicode(false);

            modelBuilder.Entity<STURUT>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.KD_CC)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.NM_PEMILIK)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.MEREK)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.TYPE)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.JENIS)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.MODEL)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.WARNA)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.THN_BUAT)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.NO_MESIN)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.NO_RANGKA)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.WARNA_TNKB)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.NO_BPKB)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.SILINDER)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.UK_CHASIS)
                .IsUnicode(false);

            modelBuilder.Entity<TRF03>()
                .Property(e => e.NO_CODE)
                .IsUnicode(false);

            modelBuilder.Entity<PBT03A>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<PBT03A>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<PBT03A>()
                .Property(e => e.NO_PO)
                .IsUnicode(false);

            modelBuilder.Entity<PBT03A>()
                .Property(e => e.POSTING)
                .IsUnicode(false);

            modelBuilder.Entity<PBT03B>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<PBT03B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<PBT03B>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<PBT03B>()
                .Property(e => e.NAMA2)
                .IsUnicode(false);

            modelBuilder.Entity<POSISISTOCK>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<POSISISTOCK>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<POSISISTOCK>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<SIF19>()
                .Property(e => e.OldSort1)
                .IsUnicode(false);

            modelBuilder.Entity<SIF19>()
                .Property(e => e.OldSort2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF19>()
                .Property(e => e.OldSort3)
                .IsUnicode(false);

            modelBuilder.Entity<SIF19>()
                .Property(e => e.OldSort4)
                .IsUnicode(false);

            modelBuilder.Entity<SIF19>()
                .Property(e => e.OldSort5)
                .IsUnicode(false);

            modelBuilder.Entity<SIF19>()
                .Property(e => e.KODE_BRG_BARU)
                .IsUnicode(false);

            modelBuilder.Entity<SIF19>()
                .Property(e => e.NewSort1)
                .IsUnicode(false);

            modelBuilder.Entity<SIF19>()
                .Property(e => e.NewSort2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF19>()
                .Property(e => e.NewSort3)
                .IsUnicode(false);

            modelBuilder.Entity<SIF19>()
                .Property(e => e.NewSort4)
                .IsUnicode(false);

            modelBuilder.Entity<SIF19>()
                .Property(e => e.NewSort5)
                .IsUnicode(false);

            modelBuilder.Entity<SIF19>()
                .Property(e => e.NO_BUKTI_MASUK_GD)
                .IsUnicode(false);

            modelBuilder.Entity<SIF19>()
                .Property(e => e.NO_BUKTI_KELUAR_GD)
                .IsUnicode(false);

            modelBuilder.Entity<SIF20A>()
                .Property(e => e.GRUP)
                .IsUnicode(false);

            modelBuilder.Entity<SIF20A>()
                .Property(e => e.KODE_EVENT)
                .IsUnicode(false);

            modelBuilder.Entity<SIF20B>()
                .Property(e => e.GRUP)
                .IsUnicode(false);

            modelBuilder.Entity<SIF20B>()
                .Property(e => e.Sort1)
                .IsUnicode(false);

            modelBuilder.Entity<SIF20B>()
                .Property(e => e.Sort2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF20B>()
                .Property(e => e.Sort3)
                .IsUnicode(false);

            modelBuilder.Entity<SIF20B>()
                .Property(e => e.Sort4)
                .IsUnicode(false);

            modelBuilder.Entity<SIF20B>()
                .Property(e => e.Sort5)
                .IsUnicode(false);

            modelBuilder.Entity<SIF20B>()
                .Property(e => e.JENIS2)
                .IsUnicode(false);

            modelBuilder.Entity<SIF20B>()
                .Property(e => e.JENIS3)
                .IsUnicode(false);

            modelBuilder.Entity<SIF20B>()
                .Property(e => e.JENIS4)
                .IsUnicode(false);

            modelBuilder.Entity<SIF20B>()
                .Property(e => e.JENIS5)
                .IsUnicode(false);

            modelBuilder.Entity<SIF20B>()
                .Property(e => e.Brg)
                .IsUnicode(false);

            modelBuilder.Entity<SIF21>()
                .Property(e => e.KODE)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.Satuan)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.DEFAULT_ENTRY)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.KODE_INVOICE)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.TRANSAKSI_ENTRY)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.JENIS_TRANS_MASUK)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.JENIS_TRANS_KELUAR)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.KD_SERI_MASUK)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.KD_SERI_KELUAR)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.METODA_PROSES_OB)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.HARGA_JUAL_ENTRY)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.Discount_Entry)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.DISCOUNT_TABLE)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.PROTEK_QOH)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.METODA_NO)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.NS_SK)
                .IsUnicode(false);

            modelBuilder.Entity<SKFSY>()
                .Property(e => e.DEFAULT_HRG_SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Level_NonSize1)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Level_NonSize2)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Level_NonSize3)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Level_NonSize4)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Level_Size)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size1)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size2)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size3)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size4)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size5)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size6)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size7)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size8)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size9)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size10)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size11)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size12)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size13)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size14)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size15)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size16)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size17)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size18)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size19)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Sort_Size20)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Level_NonSize5)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Level_NonSize6)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Level_NonSize7)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Level_NonSize8)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Level_NonSize9)
                .IsUnicode(false);

            modelBuilder.Entity<STFSYS_M>()
                .Property(e => e.Level_NonSize10)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.INV)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.JENIS)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.PO)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.POSTING)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.APP)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.NAMA)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.F_PAJAK)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.REF)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.NO_INVOICE_SUPP)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01A>()
                .Property(e => e.WO_SUBCON)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01B>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01B>()
                .Property(e => e.INV)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01B>()
                .Property(e => e.PO)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01B>()
                .Property(e => e.NAMA_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01B>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01B>()
                .Property(e => e.BK)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01B>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01B>()
                .Property(e => e.AUTO_LOAD)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01C>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01C>()
                .Property(e => e.INV)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01C>()
                .Property(e => e.NOBUK)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01C>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01D>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01D>()
                .Property(e => e.INV)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01D>()
                .Property(e => e.BIAYA)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01D>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01D>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01E>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01E>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01E>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01E>()
                .Property(e => e.LOT_NO)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01E>()
                .Property(e => e.BATCH_NO)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01E>()
                .Property(e => e.GUD)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01E>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01E>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01E>()
                .Property(e => e.SPESIFIKASI)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01F>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01F>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01F>()
                .Property(e => e.CATATAN_1)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01F>()
                .Property(e => e.CATATAN_2)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01F>()
                .Property(e => e.CATATAN_3)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01F>()
                .Property(e => e.CATATAN_4)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01F>()
                .Property(e => e.CATATAN_5)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01F>()
                .Property(e => e.CATATAN_6)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01F>()
                .Property(e => e.CATATAN_7)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01F>()
                .Property(e => e.CATATAN_8)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01F>()
                .Property(e => e.CATATAN_9)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_PBT01F>()
                .Property(e => e.CATATAN_10)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_POST>()
                .Property(e => e.BAHAN)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_POST>()
                .Property(e => e.BARANG)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.ST_POSTING)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.NO_REF)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.NO_SO)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.CUST)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.NAMA_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.KODE_ALAMAT)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.NO_KENDARAAN)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.SOPIR)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.KET)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.VLT)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.KODE_SALES)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.KODE_WIL)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.NO_F_PAJAK)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.KODE_PROYEK)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.NO_MK)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.JENIS_RETUR)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.JTRAN)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.AL3)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.AL2)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.AL1)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.AL)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.JENIS)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.CUST_QQ)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.NAMA_CUST_QQ)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.NO_FAKTUR_PPN_AR)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.NO_FAKTUR_LAMA)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.STATUS_LOADING)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.NO_FA_OUTLET)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01A>()
                .Property(e => e.NO_PO_CUST)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01B>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01B>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01B>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01B>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01B>()
                .Property(e => e.GUDANG)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01B>()
                .Property(e => e.AUTO_LOAD)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01B>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01B>()
                .Property(e => e.CATATAN)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01C>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01C>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01C>()
                .Property(e => e.NO_SJ)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01D>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01D>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01D>()
                .Property(e => e.KODE_BRG_UNIT)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01D>()
                .Property(e => e.KODE_BRG)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01D>()
                .Property(e => e.SATUAN)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01D>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01D>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01E>()
                .Property(e => e.JENISFORM)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01E>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01E>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01E>()
                .Property(e => e.LOT_NO)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01E>()
                .Property(e => e.BATCH_NO)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01E>()
                .Property(e => e.GUD)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01E>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01E>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01E>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01E>()
                .Property(e => e.SPESIFIKASI)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01F>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01F>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01F>()
                .Property(e => e.CATATAN_1)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01F>()
                .Property(e => e.CATATAN_2)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01F>()
                .Property(e => e.CATATAN_3)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01F>()
                .Property(e => e.CATATAN_4)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01F>()
                .Property(e => e.CATATAN_5)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01F>()
                .Property(e => e.CATATAN_6)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01F>()
                .Property(e => e.CATATAN_7)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01F>()
                .Property(e => e.CATATAN_8)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01F>()
                .Property(e => e.CATATAN_9)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01F>()
                .Property(e => e.CATATAN_10)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01G>()
                .Property(e => e.JENIS_FORM)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01G>()
                .Property(e => e.NO_BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01G>()
                .Property(e => e.NO_FAKTUR)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_SIT01G>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STF08>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STF08>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STF09>()
                .Property(e => e.Brg)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STF09>()
                .Property(e => e.Bukti)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STF09>()
                .Property(e => e.MK)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STF09>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STF09>()
                .Property(e => e.GD)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STF09>()
                .Property(e => e.Ref)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STF09>()
                .Property(e => e.JTran)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STF09>()
                .Property(e => e.WO)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STF09>()
                .Property(e => e.No_Faktur)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01A>()
                .Property(e => e.Nobuk)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01A>()
                .Property(e => e.Satuan)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01A>()
                .Property(e => e.Ket)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01A>()
                .Property(e => e.ST_Cetak)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01A>()
                .Property(e => e.ST_Posting)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01A>()
                .Property(e => e.JTran)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01A>()
                .Property(e => e.MK)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01A>()
                .Property(e => e.JRef)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01A>()
                .Property(e => e.Ref)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01A>()
                .Property(e => e.UserName)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01A>()
                .Property(e => e.VALUTA)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01A>()
                .Property(e => e.WORK_CENTER)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01A>()
                .Property(e => e.STATUS_LOADING)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01B>()
                .Property(e => e.Nobuk)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01B>()
                .Property(e => e.Kobar)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01B>()
                .Property(e => e.Satuan)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01B>()
                .Property(e => e.Ke_Gd)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01B>()
                .Property(e => e.Dr_Gd)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01B>()
                .Property(e => e.WO)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01B>()
                .Property(e => e.Rak)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01B>()
                .Property(e => e.JTran)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01B>()
                .Property(e => e.UserName)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01B>()
                .Property(e => e.KLINK)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01B>()
                .Property(e => e.NO_WO)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01C>()
                .Property(e => e.BUKTI)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01C>()
                .Property(e => e.BRG)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01C>()
                .Property(e => e.LOT_NO)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01C>()
                .Property(e => e.BATCH_NO)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01C>()
                .Property(e => e.GUD)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01C>()
                .Property(e => e.STN)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01C>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<TEMP_STT01C>()
                .Property(e => e.SPESIFIKASI)
                .IsUnicode(false);

            modelBuilder.Entity<tmp_STF08>()
                .Property(e => e.brg)
                .IsUnicode(false);

            modelBuilder.Entity<tmp_STF08A>()
                .Property(e => e.brg)
                .IsUnicode(false);

            modelBuilder.Entity<tmp_STF09>()
                .Property(e => e.brg)
                .IsUnicode(false);

            modelBuilder.Entity<tmp_STF09>()
                .Property(e => e.mk)
                .IsUnicode(false);

            modelBuilder.Entity<tmp_STF09>()
                .Property(e => e.jtran)
                .IsUnicode(false);

            modelBuilder.Entity<tmp_STF09A>()
                .Property(e => e.brg)
                .IsUnicode(false);

            modelBuilder.Entity<tmp_STF09A>()
                .Property(e => e.mk)
                .IsUnicode(false);

            modelBuilder.Entity<tmp_STF09A>()
                .Property(e => e.jtran)
                .IsUnicode(false);
        }
    }
}
