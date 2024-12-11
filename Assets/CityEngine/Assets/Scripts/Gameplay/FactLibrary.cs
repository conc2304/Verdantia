using System.Collections.Generic;

/**
Centralized library of categorized facts related to environmental and urban topics. 
It organizes these facts into various static lists, such as Health Benefits, Urban Reforestation Benefits, 
Urban Heat Island Effects, Pollution Effects, and others. 
These facts are stored as strings and are designed to be used in other parts of the application, 
such as carousel displays or educational features, to inform users about the positive impacts of green spaces and the challenges of urbanization.
**/
public static class FactLibrary
{
    public static readonly List<string> HealthBenefits = new List<string>
    {
        "Spending time in forests reduces cortisol levels, a key stress hormone, leading to lower stress and anxiety.",
        "Trees release phytoncides, organic compounds that boost the immune system by increasing the activity of natural killer (NK) cells.",
        "Green spaces in urban areas are linked to improved mental health, reducing the risk of depression by up to 20%.",
        "Forest exposure improves cardiovascular health by lowering blood pressure and heart rate.",
        "Walking in forest environments can enhance focus and cognitive function, particularly in children with ADHD.",
        "People living near green spaces experience fewer respiratory illnesses due to improved air quality.",
        "Urban trees filter pollutants such as nitrogen dioxide and fine particulate matter, which can reduce asthma and allergy symptoms.",
        "Access to green spaces increases opportunities for physical activity, improving overall fitness and reducing obesity rates.",
        "Forests provide natural sound buffers, reducing urban noise pollution and promoting mental well-being.",
        "Viewing greenery through a window can improve recovery times in hospital patients by up to 8%."
    };

    public static readonly List<string> UrbanReforestationBenefits = new List<string>
    {
        "Urban reforestation increases property values, with tree-lined streets boosting home prices by as much as 10%.",
        "Cities with more trees experience reduced crime rates, as green spaces foster community interactions and deter vandalism.",
        "Trees can intercept up to 30% of rainfall, reducing urban flooding by improving stormwater management.",
        "Urban forests create jobs in planting, maintenance, and education, supporting local economies.",
        "A single mature tree can absorb up to 48 pounds of carbon dioxide annually, contributing to urban climate action goals.",
        "Green roofs and vertical gardens help insulate buildings, reducing energy consumption for heating and cooling.",
        "Urban forests act as biodiversity hotspots, providing habitat for birds, bees, and other wildlife within cities.",
        "Planting trees near roads reduces vehicle noise by up to 10 decibels, creating quieter neighborhoods.",
        "Urban reforestation projects encourage environmental stewardship and community pride among residents.",
        "Cities with high tree cover are perceived as more attractive, increasing tourism and business opportunities.",
        "Urban reforestation is one of the most effective ways to combat the effects of rising temperatures.",
        "City-wide cooling improves public health, reduces energy demand, and enhances quality of life.",
        "Balancing green spaces with infrastructure is key to creating sustainable, resilient cities."
    };

    public static readonly List<string> UrbanHeatIslandEffects = new List<string>
    {
        "Trees cool the air through transpiration, where water evaporates from leaves, reducing temperatures by up to 10°F locally.",
        "Shaded surfaces in tree-covered areas can be up to 45°F cooler than those exposed to direct sunlight.",
        "Increasing urban tree canopy by 10% can reduce peak summer temperatures by 1-2°F across the city.",
        "Urban forests mitigate heat island effects by reflecting sunlight and storing less heat compared to asphalt or concrete.",
        "Trees strategically placed around buildings can cut air conditioning costs by 20-50%.",
        "Urban reforestation reduces the need for energy-intensive cooling systems, decreasing greenhouse gas emissions.",
        "Green corridors and park networks promote airflow, dispersing heat more evenly across urban areas.",
        "Large-scale urban tree planting projects have been shown to reduce city-wide mortality rates during heatwaves.",
        "Streets with dense tree cover have lower pavement temperatures, extending the lifespan of infrastructure.",
        "Cities with robust urban forests report fewer heat-related illnesses and deaths during extreme weather events."
    };

    public static readonly List<string> PollutionEffects = new List<string>
    {
        "Airborne pollutants, like black carbon and fine particulate matter (PM2.5), trap heat in the atmosphere, intensifying urban temperatures.",
        "Carbon dioxide and methane, both greenhouse gases, act as insulators in urban areas, preventing heat from dissipating into space.",
        "Pollution particles absorb solar radiation, heating the surrounding air and contributing to the 'urban pollution dome' effect.",
        "Smog and haze reduce the cooling effect of urban vegetation by blocking sunlight needed for transpiration.",
        "Polluted urban surfaces, covered in dust and soot, absorb more heat compared to clean surfaces, further warming the city.",
        "High levels of carbon emissions lead to global warming, indirectly intensifying urban heat island effects by raising baseline temperatures."
    };

    public static readonly List<string> TemperatureUpFacts = new List<string>
    {
        "High temperatures in cities increase the risk of heat-related illnesses and hospitalizations.",
        "Urban heat islands can make cities up to 7–10°F warmer than surrounding rural areas.",
        "Rising temperatures accelerate energy demand for air conditioning, raising utility costs and emissions.",
        "Heatwaves are becoming more frequent and intense due to climate change, amplifying urban heat effects.",
        "Dark surfaces like asphalt and rooftops absorb and retain heat, increasing city temperatures.",
        "Pollution traps heat in the atmosphere, worsening urban heat island effects.",
        "Higher city temperatures reduce air quality by increasing ground-level ozone and smog.",
        "Extreme heat stresses infrastructure, reducing the lifespan of roads, bridges, and buildings.",
        "The lack of tree cover and green spaces in urban areas limits natural cooling mechanisms like shade and transpiration.",
        "Unshaded pavement can reach temperatures of 120–150°F, radiating heat into the surrounding air."
    };

    public static readonly List<string> TemperatureDownFacts = new List<string> {
        "Urban green spaces and tree cover can lower temperatures by 2–9°F through shade and evaporation.",
        "Planting trees strategically around buildings can reduce cooling costs by up to 50%.",
        "Parks and green roofs mitigate heat by absorbing sunlight and releasing moisture into the air.",
        "Reflective or light-colored surfaces help cool cities by reflecting sunlight instead of absorbing it.",
        "Replacing asphalt with permeable surfaces reduces heat absorption and improves stormwater management.",
        "Urban reforestation reduces heat islands by increasing shade and enhancing air circulation.",
        "Cooler cities are more energy-efficient, reducing emissions from air conditioning use.",
        "Green corridors promote airflow, dispersing heat and improving overall urban temperature regulation.",
        "Reducing pollution levels improves air quality and allows vegetation to cool the environment more effectively.",
        "A single large tree can cool an area equivalent to 10 air conditioners running for 20 hours a day."
    };
}
