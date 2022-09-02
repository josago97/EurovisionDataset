# Eurovision Song Contest Dataset
This repository is a freely accessible data set that contains information about the participants and votes of all the editions of the Eurovision Song Contest.

Every year the data set will be updated with the results of the contest, from the first edition in 1956 until now.

The data is obtained from the [Eschome](https://eschome.net/) and [EurovisionWorld](https://eurovisionworld.com) websites.

## Downloading the dataset
The dataset can be downloaded [here](https://github.com/josago97/EurovisionDataset/releases) or from the *eurovision.json* file found in the main branch.

## Data description
The dataset is in JSON format.

### Eurovision
It is the root of the dataset.
| Attribute | Type|  Description |  
|---|---|---|
| countries | Dictionary<string, string> | Relationship between the codes and the names of the countries that have ever participated in the contest |
| contests | Contest[] | All editions of the contest | 

### Contest
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

### Contestant
| Attribute | Type|  Description |  
|---|---|---|
| id | integer | Contestant ID (used in Performance ) |
| country | string | Code of the country that is represented |
| artist | string | Name of the singer/group performing |
| song | string | Song title |
| composers | string[] | Song composers |
| writers | string[] | Song lyricists |
| lyrics | string | Lyrics of the song (in the corresponding language) |
| videoUrl | string | Link to a Youtube video showing the song |
| broadcaster | string | Candidate country broadcaster|

### Round
| Attribute | Type|  Description |  
|---|---|---|
| name | string | Round name (final, semifinal if the year is between 2004 and 2007, semifinal1 or semifinal2 if the year is greater than 2007  ) |
| date | DateTime | Date and time the round took place |
| performances | Performance[] | Results of the performances of the contestants in this round |

### Performance
| Attribute | Type|  Description |  
|---|---|---|
| contestantId | integer | Contestant ID |
| running | integer | Place on the running |
| place | integer | Place in the ranking |
| scores | Score[] | Score and voting |

### Score
| Attribute | Type|  Description |  
|---|---|---|
| name | string | Origin of points (total, tele and jury if the year is greater than 2015) |
| points | integer | Total points earned |
| votes | Dictionary<string, integer> | Votes received from each country (using the country code) |
