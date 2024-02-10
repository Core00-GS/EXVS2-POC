﻿using System.Net.Http.Json;
using Throw;
using WebUIVanilla.Shared.Dto.Common;

namespace WebUIVanilla.Client.Services;

public class DataService : IDataService
{
    private Dictionary<uint, IdValuePair> displayOptions = new();
    private Dictionary<uint, IdValuePair> echelonDisplayOptions = new();
    private Dictionary<uint, MobileSuit> mobileSuits = new();
    private Dictionary<uint, IdValuePair> bgm = new();
    private Dictionary<uint, IdValuePair> gauge = new();
    private Dictionary<uint, Familiarity> msFamiliarities = new();
    private Dictionary<uint, Familiarity> naviFamiliarities = new();
    private Dictionary<uint, Navigator> navigator = new();
    private Dictionary<uint, IdValuePair> triadSkill = new();
    private Dictionary<uint, IdValuePair> gamepadOptions = new();
    private Dictionary<uint, IdValuePair> customizeCommentSentences = new();
    private Dictionary<uint, IdValuePair> customizeCommentPhrases = new();
    private Dictionary<uint, GeneralPreview> stamps = new();
    private Dictionary<uint, GeneralPreview> titles = new();
    private Dictionary<uint, GeneralPreview> backgrounds = new();
    private Dictionary<uint, GeneralPreview> effects = new();
    private Dictionary<uint, GeneralPreview> ornaments = new();
    private Dictionary<uint, GeneralPreview> teamNameFontColors = new();
    private Dictionary<uint, GeneralPreview> teamBackgrounds = new();
    private Dictionary<uint, GeneralPreview> teamEffects = new();
    private Dictionary<uint, GeneralPreview> teamEmblems = new();
    private Dictionary<uint, TagTeamMastery> tagTeamMasteries = new();

    private List<IdValuePair> sortedDisplayOptionList = new();
    private List<IdValuePair> sortedEchelonDisplayOptionList = new();
    private List<MobileSuit> sortedMobileSuitList = new();
    private List<IdValuePair> sortedBgmList = new();
    private List<IdValuePair> sortedGaugeList = new();
    private List<Familiarity> sortedMsFamiliarityList = new();
    private List<Familiarity> sortedNaviFamiliarityList = new();
    private List<Navigator> sortedNavigatorList = new();
    private List<IdValuePair> sortedTriadSkillList = new();
    private List<IdValuePair> sortedGamepadOptionList = new();
    private List<TriadCourseConfig> triadCourseConfigList = new();
    private List<IdValuePair> customizeCommentSentenceList = new();
    private List<IdValuePair> customizeCommentPhraseList = new();
    private List<GeneralPreview> sortedStampsList = new();
    private List<GeneralPreview> sortedTitleList = new();
    private List<GeneralPreview> sortedBackgroundList = new();
    private List<GeneralPreview> sortedEffectList = new();
    private List<GeneralPreview> sortedOrnamentList = new();
    private List<GeneralPreview> sortedTeamNameFontColorList = new();
    private List<GeneralPreview> sortedTeamBackgroundList = new();
    private List<GeneralPreview> sortedTeamEffectList = new();
    private List<GeneralPreview> sortedTeamEmblemList = new();
    private List<TagTeamMastery> sortedTagTeamMasteryList = new();

    private readonly HttpClient client;
    private readonly ILogger<DataService> logger;

    public DataService(HttpClient client, ILogger<DataService> logger)
    {
        this.client = client;
        this.logger = logger;
    }

    public async Task InitializeAsync()
    {
        var displayOptionList = await client.GetFromJsonAsync<List<IdValuePair>>("data/DisplayOptions.json");
        displayOptionList.ThrowIfNull();
        displayOptions = displayOptionList.ToDictionary(pair => pair.Id);
        sortedDisplayOptionList = displayOptionList.OrderBy(title => title.Id).ToList();
        
        var echelonDisplayOptionList = await client.GetFromJsonAsync<List<IdValuePair>>("data/EchelonDisplayOptions.json");
        echelonDisplayOptionList.ThrowIfNull();
        echelonDisplayOptions = echelonDisplayOptionList.ToDictionary(pair => pair.Id);
        sortedEchelonDisplayOptionList = echelonDisplayOptionList.OrderBy(title => title.Id).ToList();
        
        var msFamiliarityList = await client.GetFromJsonAsync<List<Familiarity>>("data/MobileSuitFamiliarity.json");
        msFamiliarityList.ThrowIfNull();
        msFamiliarities = msFamiliarityList.ToDictionary(msFamiliarity => msFamiliarity.Id);
        sortedMsFamiliarityList = msFamiliarityList.OrderBy(msFamiliarity => msFamiliarity.Id).ToList();
        
        var msList = await client.GetFromJsonAsync<List<MobileSuit>>("data/MobileSuits.json");
        msList.ThrowIfNull();
        mobileSuits = msList.ToDictionary(ms => ms.Id);
        sortedMobileSuitList = msList.OrderBy(title => title.Id).ToList();

        var bgmList = await client.GetFromJsonAsync<List<IdValuePair>>("data/Bgms.json");
        bgmList.ThrowIfNull();
        bgm = bgmList.ToDictionary(ms => ms.Id);
        sortedBgmList = bgmList.OrderBy(title => title.Id).ToList();

        var gaugeList = await client.GetFromJsonAsync<List<IdValuePair>>("data/Gauges.json");
        gaugeList.ThrowIfNull();
        gauge = gaugeList.ToDictionary(ms => ms.Id);
        sortedGaugeList = gaugeList.OrderBy(title => title.Id).ToList();
        
        var naviFamiliarityList = await client.GetFromJsonAsync<List<Familiarity>>("data/NaviFamiliarity.json");
        naviFamiliarityList.ThrowIfNull();
        naviFamiliarities = naviFamiliarityList.ToDictionary(naviFamiliarity => naviFamiliarity.Id);
        sortedNaviFamiliarityList = naviFamiliarityList.OrderBy(naviFamiliarity => naviFamiliarity.Id).ToList();

        var naviList = await client.GetFromJsonAsync<List<Navigator>>("data/Navigators.json");
        naviList.ThrowIfNull();
        navigator = naviList.ToDictionary(ms => ms.Id);
        sortedNavigatorList = naviList.OrderBy(title => title.Id).ToList();
        
        var triadSkillList = await client.GetFromJsonAsync<List<IdValuePair>>("data/TriadSkills.json");
        triadSkillList.ThrowIfNull();
        triadSkill = triadSkillList.ToDictionary(ms => ms.Id);
        sortedTriadSkillList = triadSkillList.OrderBy(title => title.Id).ToList();
        
        var gamepadOptionList = await client.GetFromJsonAsync<List<IdValuePair>>("data/GamepadOptions.json");
        gamepadOptionList.ThrowIfNull();
        gamepadOptions = gamepadOptionList.ToDictionary(gamepadOption => gamepadOption.Id);
        sortedGamepadOptionList = gamepadOptionList.OrderBy(gamepadOption => gamepadOption.Id).ToList();
        
        var triadCourseConfigs = await client.GetFromJsonAsync<List<TriadCourseConfig>>("data/TriadCourseConfigs.json");
        triadCourseConfigs.ThrowIfNull();
        triadCourseConfigList = triadCourseConfigs;
        
        var customizeCommentSentencesList = await client.GetFromJsonAsync<List<IdValuePair>>("data/CustomizeComment.json");
        customizeCommentSentencesList.ThrowIfNull();
        customizeCommentSentences = customizeCommentSentencesList.ToDictionary(ms => ms.Id);
        customizeCommentSentenceList = customizeCommentSentencesList.OrderBy(title => title.Id).ToList();
        
        var customizeCommentPhrasesList = await client.GetFromJsonAsync<List<IdValuePair>>("data/CustomizeCommentPhrase.json");
        customizeCommentPhrasesList.ThrowIfNull();
        customizeCommentPhrases = customizeCommentPhrasesList.ToDictionary(ms => ms.Id);
        customizeCommentPhraseList = customizeCommentPhrasesList.OrderBy(title => title.Id).ToList();
        
        var stampList = await client.GetFromJsonAsync<List<GeneralPreview>>("data/Stamps.json");
        stampList.ThrowIfNull();
        stamps = CreateGeneralPreviewDictionary(stampList);
        sortedStampsList = CreateSortedGeneralPreviewList(stampList);
        
        var titleList = await client.GetFromJsonAsync<List<GeneralPreview>>("data/titles/Titles.json");
        titleList.ThrowIfNull();
        titles = CreateGeneralPreviewDictionary(titleList);
        sortedTitleList = CreateSortedGeneralPreviewList(titleList);
        
        var backgroundList = await client.GetFromJsonAsync<List<GeneralPreview>>("data/titles/Backgrounds.json");
        backgroundList.ThrowIfNull();
        backgrounds = CreateGeneralPreviewDictionary(backgroundList);
        sortedBackgroundList = CreateSortedGeneralPreviewList(backgroundList);
        
        var effectList = await client.GetFromJsonAsync<List<GeneralPreview>>("data/titles/Effects.json");
        effectList.ThrowIfNull();
        effects = CreateGeneralPreviewDictionary(effectList);
        sortedEffectList = CreateSortedGeneralPreviewList(effectList);
        
        var ornamentList = await client.GetFromJsonAsync<List<GeneralPreview>>("data/titles/Ornaments.json");
        ornamentList.ThrowIfNull();
        ornaments = CreateGeneralPreviewDictionary(ornamentList);
        sortedOrnamentList = CreateSortedGeneralPreviewList(ornamentList);
        
        var teamBackgroundList = await client.GetFromJsonAsync<List<GeneralPreview>>("data/team/Backgrounds.json");
        teamBackgroundList.ThrowIfNull();
        teamBackgrounds = CreateGeneralPreviewDictionary(teamBackgroundList);
        sortedTeamBackgroundList = CreateSortedGeneralPreviewList(teamBackgroundList);
        
        var teamEffectList = await client.GetFromJsonAsync<List<GeneralPreview>>("data/team/Effects.json");
        teamEffectList.ThrowIfNull();
        teamEffects = CreateGeneralPreviewDictionary(teamEffectList);
        sortedTeamEffectList = CreateSortedGeneralPreviewList(teamEffectList);
        
        var teamEmblemList = await client.GetFromJsonAsync<List<GeneralPreview>>("data/team/Emblems.json");
        teamEmblemList.ThrowIfNull();
        teamEmblems = CreateGeneralPreviewDictionary(teamEmblemList);
        sortedTeamEmblemList = CreateSortedGeneralPreviewList(teamEmblemList);
        
        var teamFontColorList = await client.GetFromJsonAsync<List<GeneralPreview>>("data/team/NameFontColors.json");
        teamFontColorList.ThrowIfNull();
        teamNameFontColors = CreateGeneralPreviewDictionary(teamFontColorList);
        sortedTeamNameFontColorList = CreateSortedGeneralPreviewList(teamFontColorList);
        
        var tagTeamMasteryList = await client.GetFromJsonAsync<List<TagTeamMastery>>("data/team/Mastery.json");
        tagTeamMasteryList.ThrowIfNull();
        tagTeamMasteries = tagTeamMasteryList.ToDictionary(mastery => mastery.Id);
        sortedTagTeamMasteryList = tagTeamMasteryList.OrderBy(mastery => mastery.Id).ToList();
    }
    
    private Dictionary<uint, GeneralPreview> CreateGeneralPreviewDictionary(List<GeneralPreview> generalPreviews)
    {
        return generalPreviews
            .Where(generalPreview => generalPreview.Existence != "NotExist")
            .ToDictionary(generalPreview => generalPreview.Id);
    }

    private List<GeneralPreview> CreateSortedGeneralPreviewList(List<GeneralPreview> generalPreviews)
    {
        return generalPreviews
            .Where(generalPreview => generalPreview.Existence != "NotExist")  
            .OrderBy(generalPreview => generalPreview.Id)
            .ToList();
    }

    public IReadOnlyList<IdValuePair> GetDisplayOptionsSortedById()
    {
        return sortedDisplayOptionList;
    }
    
    public IReadOnlyList<IdValuePair> GetEchelonDisplayOptionsSortedById()
    {
        return sortedEchelonDisplayOptionList;
    }
    
    public IReadOnlyList<MobileSuit> GetMobileSuitSortedById()
    {
        return sortedMobileSuitList;
    }
    
    public List<MobileSuit> GetWritableMobileSuitSortedById()
    {
        return sortedMobileSuitList;
    }
    
    public IReadOnlyList<MobileSuit> GetCostumeMobileSuitSortedById()
    {
        return sortedMobileSuitList.FindAll(ms => ms.Costumes != null && ms.Costumes.Count > 1);
    }

    public MobileSuit? GetMobileSuitById(uint id)
    {
        return mobileSuits.GetValueOrDefault(id);
    }

    public IReadOnlyList<IdValuePair> GetBgmSortedById()
    {
        return sortedBgmList;
    }

    public IdValuePair? GetBgmById(uint id)
    {
        return bgm.GetValueOrDefault(id);
    }

    public IReadOnlyList<IdValuePair> GetGaugeSortedById()
    {
        return sortedGaugeList;
    }

    public IdValuePair? GetGaugeById(uint id)
    {
        return gauge.GetValueOrDefault(id);
    }
    
    public IReadOnlyList<Familiarity> GetMsFamiliaritySortedById()
    {
        return sortedMsFamiliarityList;
    }

    public IReadOnlyList<Familiarity> GetNaviFamiliaritySortedById()
    {
        return sortedNaviFamiliarityList;
    }

    public IReadOnlyList<Navigator> GetNavigatorSortedById()
    {
        return sortedNavigatorList;
    }
    
    public List<Navigator> GetWritableNavigatorSortedById()
    {
        return sortedNavigatorList;
    }
    
    public Navigator? GetNavigatorById(uint id)
    {
        return navigator.GetValueOrDefault(id);
    }
    public IReadOnlyList<IdValuePair> GetSortedTriadSkillList()
    {
        return sortedTriadSkillList;
    }
    public IdValuePair? GetTriadSkill(uint id)
    {
        return triadSkill.GetValueOrDefault(id);
    }
    
    public IReadOnlyList<IdValuePair> GetSortedGamepadOptionList()
    {
        return sortedGamepadOptionList;
    }
    
    public List<TriadCourseConfig> GetTriadStageConfigs()
    {
        return triadCourseConfigList;
    }

    public IdValuePair? GetCustomizeCommentSentenceById(uint id)
    {
        return customizeCommentSentences.GetValueOrDefault(id);
    }

    public IReadOnlyList<IdValuePair> GetCustomizeCommentSentenceSortedById()
    {
        return customizeCommentSentenceList;
    }
    
    public IdValuePair? GetCustomizeCommentPhraseById(uint id)
    {
        return customizeCommentPhrases.GetValueOrDefault(id);
    }

    public IReadOnlyList<IdValuePair> GetCustomizeCommentPhraseSortedById()
    {
        return customizeCommentPhraseList;
    }
    
    public GeneralPreview? GetStampById(uint id)
    {
        return stamps.GetValueOrDefault(id);
    }
    
    public IReadOnlyList<GeneralPreview> GetStampsSortedById()
    {
        return sortedStampsList;
    }
    
    public GeneralPreview? GetTitleById(uint id)
    {
        return titles.GetValueOrDefault(id);
    }
    
    public IReadOnlyList<GeneralPreview> GetTitlesSortedById()
    {
        return sortedTitleList;
    }
    
    public GeneralPreview? GetBackgroundById(uint id)
    {
        return backgrounds.GetValueOrDefault(id);
    }
    
    public IReadOnlyList<GeneralPreview> GetBackgroundsSortedById()
    {
        return sortedBackgroundList;
    }
    
    public GeneralPreview? GetEffectById(uint id)
    {
        return effects.GetValueOrDefault(id);
    }
    
    public IReadOnlyList<GeneralPreview> GetEffectsSortedById()
    {
        return sortedEffectList;
    }
    
    public GeneralPreview? GetOrnamentById(uint id)
    {
        return ornaments.GetValueOrDefault(id);
    }
    
    public IReadOnlyList<GeneralPreview> GetOrnamentsSortedById()
    {
        return sortedOrnamentList;
    }
    
    public GeneralPreview? GetTeamNameFontColorById(uint id)
    {
        return teamNameFontColors.GetValueOrDefault(id);
    }
    
    public IReadOnlyList<GeneralPreview> GetTeamNameFontColorsSortedById()
    {
        return sortedTeamNameFontColorList;
    }
    
    public GeneralPreview? GetTeamEffectById(uint id)
    {
        return teamEffects.GetValueOrDefault(id);
    }
    
    public IReadOnlyList<GeneralPreview> GetTeamEffectsSortedById()
    {
        return sortedTeamEffectList;
    }
    
    public GeneralPreview? GetTeamEmblemById(uint id)
    {
        return teamEmblems.GetValueOrDefault(id);
    }
    
    public IReadOnlyList<GeneralPreview> GetTeamEmblemsSortedById()
    {
        return sortedTeamEmblemList;
    }
    public GeneralPreview? GetTeamBackgroundById(uint id)
    {
        return teamBackgrounds.GetValueOrDefault(id);
    }
    
    public IReadOnlyList<GeneralPreview> GetTeamBackgroundsSortedById()
    {
        return sortedTeamBackgroundList;
    }
    
    public TagTeamMastery? GetTagTeamMasteryById(uint id)
    {
        return tagTeamMasteries.GetValueOrDefault(id);
    }
    
    public IReadOnlyList<TagTeamMastery> GetTagTeamMasterySortedById()
    {
        return sortedTagTeamMasteryList;
    }
}