using System;
using System.Collections.Generic;
using System.Text;

namespace DressmakerArchipelago
{
    public class DressmakerDump
    {
        public List<StoryQuestInfo> storyQuestInfos;
        public List<SideQuestInfo> sideQuestInfos;
        public List<FabricInfo> fabricInfos;
        public List<PatternInfo> patternInfos;
        public FinalQuestInfo finalQuestInfo;
    }
    public class StoryQuestInfo
    {
        public string name;
        public string prettyName;
        public string questGiverCharacterName;
    }
    public class FinalQuestInfo
    {
        public string name;
        public string prettyName;
        public string questGiverCharacterName;
    }
    public class SideQuestInfo
    {
        public string name;
        public string prettyName;
        public string questGiverCharacterName;
    }
    public class FabricInfo
    {
        public string name;
        public string prettyName;
        public Dictionary<string, float> itemTags;

    }

    public class PatternInfo
    {
        public string PatternType;
        public string name;
        public string prettyName;
        public Dictionary<string, float> itemTags;
    }

    public class ActiveDress
    {
        public string collarName;
        public string bodiceName;
        public string sleeveName;
        public string skirtName;
    }

}
