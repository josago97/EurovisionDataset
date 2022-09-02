# Eurovision Song Contest Dataset

## Data description

### Eurovision
It is the root of the dataset.
| Attribute | Type|  Description |  
|---|---|---|
| countries | Dictionary<string, string> | Relationship between the codes and the names of the countries that have ever participated in the contest |
| contests | Contest[] | All editions of the contest | 

### Contest
It contains the data of the contest of a certain year.
| Attribute | Type|  Description |  
|---|---|---|
| year | integer | Year in which the contest was held |
| arena | string | Building where the contest was held |
| city | string | Host city |
| country | string | Host country code |
| broadcasters | string[] | Host broadcasters of the edition |
| presenters | string[] | Presenters of the contest |
| slogan | string | Slogan of the edition |
| contestants | Contestant[] | All the contestants of the edition |
| rounds | Round[] | All rounds of the contest |
