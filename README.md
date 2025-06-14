# ❤ Unofficial Eurovision Song Contest Dataset ❤

## This repository is transferred [here](https://github.com/EurovisionAPI/dataset/).

This repository is a freely accessible dataset that contains information about the participants and votes of all editions of the Eurovision Song Contest and Junior Eurovision Song Contest.

Every year the dataset will be updated with the results of the contest, from the first edition in 1956 to the present. For Junior Eurovision, the first edition was in 2003.

The data is obtained from the [ESC Home](https://eschome.net/), [Eurovision World](https://eurovisionworld.com), [Eurovision LOD](https://so-we-must-think.space/greenstone3/eurovision-library/collection/eurovision/page/about), [Logopedia](https://logos.fandom.com/) and [Ogaespain](https://www.ogaespain.com/) websites.

You can see the dataset information on this [web page](https://josago97.github.io/EurovisionDataset/).

You can also access the data through [this api](https://eurovisionapi.runasp.net/).

## Downloading the dataset

The dataset can be downloaded [here](https://github.com/josago97/EurovisionDataset/releases) or from the [dataset folder](https://github.com/josago97/EurovisionDataset/tree/main/dataset).

## Data description

The dataset is in JSON format.

## Countries (countries.json)

A dictionary of strings with the relationship between the codes and the names of the countries that have ever participated in the contest.

## Eurovision (eurovision.json)

An array of Contest representing all the editions of the contest.

### Contest

It represents an annual edition.
| Attribute | Type | Description |  
|---|---|---|
| year | integer | Year in which the contest was held |
| arena | string | Building where the contest was held |
| city | string | Host city |
| country | string | Host country code |
| intendedCountry | string | If not null stores the code of the country that should have been the host but couldn't (Ukraine 2023)
| slogan | string | Slogan of the contest |
| logoUrl | string | Link to contest thumbnail |
| voting | string | Information about the voting system |
| presenters | string[] | Presenters of the edition |
| broadcasters | string[] | Host broadcasters of the contest |
| contestants | Contestant[] | All contestants of the contest |
| rounds | Round[] | All rounds of the contest |

### Contestant

It represents each of the contestant songs of the edition.
| Attribute | Type | Description |  
|---|---|---|
| id | integer | Contestant ID (used in Performance ) |
| country | string | Code of the country that is represented |
| artist | string | Name of the singer/group performing |
| song | string | Song title |
| lyrics | Lyrics[] | All lyrics of the song with translations (in the corresponding language). The first lyrics is the original. |
| videoUrl | string[] | All links to a Youtube videos showing the song |
| tone | string | Key and scale of the song |
| bpm | integer | Beats per minute of the song |
| dancers | string[] | Song dancers |
| backings | string[] | Song backings |
| jury | string[] | Jury in song selection |
| composers | string[] | Song composers |
| lyricists | string[] | Song lyricists |
| writers | string[] | Song writers |
| conductor | string | Song conductor |
| stageDirector | string | Song stage director |
| broadcaster | string | Candidate country broadcaster |
| spokesperson | string | Candidate country spokesperson |
| commentators | string[] | Candidate country commentators|

### Lyrics

It represents the original lyrics of the song and each of the translations of the lyrics (indicating their languages).
| Attribute | Type | Description |  
|---|---|---|
| languages | string[] | All languages that contains the song lyrics |
| title | string | The song title |
| content | string | The song lyrics, paragraphs are separated by double line break ("\n\n")|

### Round

| Attribute     | Type          | Description                                                                                                                    |
| ------------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| name          | string        | Round name (final, semifinal if the year is between 2004 and 2007, semifinal1 or semifinal2 if the year is greater than 2007 ) |
| date          | string        | Date when the round was held in UTC                                                                                            |
| time          | string        | Time when the round was held in UTC                                                                                            |
| performances  | Performance[] | Results of the performances of the contestants in this round                                                                   |
| disqualifieds | int[]         | The id of the contestants who have been disqualified in the round                                                              |

### Performance

| Attribute    | Type    | Description          |
| ------------ | ------- | -------------------- |
| contestantId | integer | Contestant ID        |
| running      | integer | Place on the running |
| place        | integer | Place in the ranking |
| scores       | Score[] | Score and voting     |

### Score

| Attribute | Type                        | Description                                                              |
| --------- | --------------------------- | ------------------------------------------------------------------------ |
| name      | string                      | Origin of points (total, tele and jury if the year is greater than 2015) |
| points    | integer                     | Total points earned                                                      |
| votes     | Dictionary<string, integer> | Votes received from each country (using the country code)                |

## Junior Eurovision (junior.json)

An array of Contest representing all the editions of the junior contest.

### Contest

It represents an annual edition.
| Attribute | Type | Description |  
|---|---|---|
| year | integer | Year in which the contest was held |
| arena | string | Building where the contest was held |
| city | string | Host city |
| country | string | Host country code |
| slogan | string | Slogan of the contest |
| logoUrl | string | Link to contest thumbnail |
| voting | string | Information about the voting system |
| presenters | string[] | Presenters of the edition |
| contestants | Contestant[] | All contestants of the contest |
| rounds | Round[] | All rounds of the contest |

### Contestant

It represents each of the contestant songs of the edition.
| Attribute | Type | Description |  
|---|---|---|
| id | integer | Contestant ID (used in Performance ) |
| country | string | Code of the country that is represented |
| artist | string | Name of the singer/group performing |
| song | string | Song title |
| lyrics | Lyrics[] | All lyrics of the song with translations (in the corresponding language). The first lyrics is the original. |
| videoUrl | string[] | All links to a Youtube videos showing the song |
| dancers | string[] | Song dancers |
| backings | string[] | Song backings |
| composers | string[] | Song composers |
| lyricists | string[] | Song lyricists |
| writers | string[] | Song writers |

### Lyrics

It represents the original lyrics of the song and each of the translations of the lyrics (indicating their languages).
| Attribute | Type | Description |  
|---|---|---|
| languages | string[] | All languages that contains the song lyrics |
| title | string | The song title |
| content | string | The song lyrics, paragraphs are separated by double line break ("\n\n")|

### Round

| Attribute    | Type          | Description                                                  |
| ------------ | ------------- | ------------------------------------------------------------ |
| name         | string        | Round name (finals only for now)                             |
| date         | string        | Date when the round was held in UTC                          |
| time         | string        | Time when the round was held in UTC                          |
| performances | Performance[] | Results of the performances of the contestants in this round |

### Performance

| Attribute    | Type    | Description          |
| ------------ | ------- | -------------------- |
| contestantId | integer | Contestant ID        |
| running      | integer | Place on the running |
| place        | integer | Place in the ranking |
| scores       | Score[] | Score and voting     |

### Score

| Attribute | Type    | Description                                             |
| --------- | ------- | ------------------------------------------------------- |
| name      | string  | Origin of points (total, nationals, online, kids, etc.) |
| points    | integer | Points earned                                           |
