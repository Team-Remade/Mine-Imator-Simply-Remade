using System;
using System.Collections.Generic;
using System.IO;
using MineImatorSimplyRemade.app.dialogues;
using Newtonsoft.Json;

namespace MineImatorSimplyRemade.utility.file.json;

public static class Json
{
    public static JsonFile Load(string file)
    {
        var jsonFile = new JsonFile();

        try
        {
            var json = File.Exists(file) ? File.ReadAllText(file) : "";
            
            jsonFile = JsonConvert.DeserializeObject<JsonFile>(json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ErrorWindow.ShowMessage(e.Message);
        }
        
        return jsonFile;
    }
}

public class JsonFile
{
    
}

public class LegacyData : JsonFile
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class _0x1
    {
        [JsonProperty("0")]
        public string _0 { get; set; }

        [JsonProperty("1")]
        public string _1 { get; set; }
    }

    public class _0x10x2
    {
        [JsonProperty("0")]
        public string _0 { get; set; }

        [JsonProperty("1")]
        public string _1 { get; set; }

        [JsonProperty("2")]
        public string _2 { get; set; }

        [JsonProperty("3")]
        public string _3 { get; set; }
    }

    public class _0x10x20x4
    {
        [JsonProperty("0")]
        public string _0 { get; set; }

        [JsonProperty("1")]
        public string _1 { get; set; }

        [JsonProperty("2")]
        public string _2 { get; set; }

        [JsonProperty("3")]
        public string _3 { get; set; }

        [JsonProperty("4")]
        public string _4 { get; set; }

        [JsonProperty("5")]
        public string _5 { get; set; }

        [JsonProperty("6")]
        public string _6 { get; set; }

        [JsonProperty("7")]
        public string _7 { get; set; }

        [JsonProperty("8")]
        public string _8 { get; set; }

        [JsonProperty("9")]
        public string _9 { get; set; }
    }

    public class _0x2
    {
        [JsonProperty("0")]
        public string _0 { get; set; }

        [JsonProperty("1")]
        public string _1 { get; set; }
    }

    public class _0x4
    {
        [JsonProperty("0")]
        public string _0 { get; set; }

        [JsonProperty("1")]
        public string _1 { get; set; }
    }

    public class _0x40x8
    {
        [JsonProperty("0")]
        public string _0 { get; set; }

        [JsonProperty("1")]
        public string _1 { get; set; }

        [JsonProperty("2")]
        public string _2 { get; set; }

        [JsonProperty("3")]
        public string _3 { get; set; }
    }

    public class _0x8
    {
        [JsonProperty("0")]
        public string _0 { get; set; }

        [JsonProperty("1")]
        public string _1 { get; set; }
    }

    public class _1
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _10
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _100
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _101
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _102
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _103
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _104
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _105
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _106
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _107
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _108
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _109
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _11
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _110
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _111
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _112
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _113
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _114
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _115
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _116
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _117
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _118
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _12
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _120
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _121
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _122
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _123
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _124
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _125
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _126
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _127
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _128
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _129
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _13
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _130
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _131
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _132
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _133
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _134
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _135
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _136
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _137
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _138
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _139
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _14
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _140
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _141
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _142
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _143
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _145
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _146
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _147
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _148
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _149
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _15
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _150
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _151
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _152
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _153
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _154
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _155
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _156
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _157
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _158
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _159
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _16
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _160
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _161
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _162
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _163
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _164
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _165
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _167
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _168
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _169
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _17
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _170
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _171
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _172
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _173
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _174
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _175
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _176
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _177
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _178
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _179
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _18
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _180
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _181
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _182
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _183
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _184
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _185
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _186
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _187
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _188
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _189
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _19
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _190
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _191
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _192
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _193
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _194
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _195
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _196
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _197
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _198
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _199
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _2
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _20
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _200
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _201
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _202
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _203
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _204
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _205
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _206
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _207
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _208
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _21
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _210
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _211
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _212
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _213
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _214
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _215
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _216
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _218
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _219
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _22
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _220
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _221
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _222
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _223
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _224
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _225
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _226
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _227
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _228
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _229
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _23
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _230
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _231
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _232
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _233
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _234
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _235
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _236
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _237
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _238
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _239
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _24
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _240
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _241
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _242
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _243
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _244
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _245
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _246
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _247
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _248
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _249
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _25
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _250
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _251
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _252
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _255
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _26
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _27
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _28
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _29
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _3
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _30
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _31
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _32
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _33
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _35
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _37
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _38
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _39
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _4
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _40
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _41
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _42
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _43
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _44
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _45
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _46
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _47
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _48
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _49
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _5
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _50
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _51
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _52
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _53
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _54
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _55
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _56
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _57
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _58
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _59
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _6
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _60
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _61
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _62
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _63
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _64
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _65
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _66
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _67
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _68
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _69
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _7
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _70
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _71
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _72
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _73
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _74
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _75
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _76
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _77
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _78
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _79
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _8
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _80
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _81
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _82
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _83
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _84
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _85
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _86
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _87
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _88
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _89
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class _9
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _90
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _91
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _92
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _93
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _94
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _95
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _96
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _97
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _98
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class _99
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class Alex
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class BiomeIds
    {
        [JsonProperty("0")]
        public string _0 { get; set; }

        [JsonProperty("24")]
        public string _24 { get; set; }

        [JsonProperty("10")]
        public string _10 { get; set; }

        [JsonProperty("50")]
        public string _50 { get; set; }

        [JsonProperty("46")]
        public string _46 { get; set; }

        [JsonProperty("49")]
        public string _49 { get; set; }

        [JsonProperty("45")]
        public string _45 { get; set; }

        [JsonProperty("48")]
        public string _48 { get; set; }

        [JsonProperty("44")]
        public string _44 { get; set; }

        [JsonProperty("7")]
        public string _7 { get; set; }

        [JsonProperty("11")]
        public string _11 { get; set; }

        [JsonProperty("16")]
        public string _16 { get; set; }

        [JsonProperty("25")]
        public string _25 { get; set; }

        [JsonProperty("26")]
        public string _26 { get; set; }

        [JsonProperty("4")]
        public string _4 { get; set; }

        [JsonProperty("132")]
        public string _132 { get; set; }

        [JsonProperty("27")]
        public string _27 { get; set; }

        [JsonProperty("155")]
        public string _155 { get; set; }

        [JsonProperty("29")]
        public string _29 { get; set; }

        [JsonProperty("21")]
        public string _21 { get; set; }

        [JsonProperty("23")]
        public string _23 { get; set; }

        [JsonProperty("168")]
        public string _168 { get; set; }

        [JsonProperty("5")]
        public string _5 { get; set; }

        [JsonProperty("30")]
        public string _30 { get; set; }

        [JsonProperty("32")]
        public string _32 { get; set; }

        [JsonProperty("160")]
        public string _160 { get; set; }

        [JsonProperty("14")]
        public string _14 { get; set; }

        [JsonProperty("6")]
        public string _6 { get; set; }

        [JsonProperty("35")]
        public string _35 { get; set; }

        [JsonProperty("36")]
        public string _36 { get; set; }

        [JsonProperty("163")]
        public string _163 { get; set; }

        [JsonProperty("1")]
        public string _1 { get; set; }

        [JsonProperty("129")]
        public string _129 { get; set; }

        [JsonProperty("2")]
        public string _2 { get; set; }

        [JsonProperty("12")]
        public string _12 { get; set; }

        [JsonProperty("140")]
        public string _140 { get; set; }

        [JsonProperty("3")]
        public string _3 { get; set; }

        [JsonProperty("34")]
        public string _34 { get; set; }

        [JsonProperty("131")]
        public string _131 { get; set; }

        [JsonProperty("37")]
        public string _37 { get; set; }

        [JsonProperty("38")]
        public string _38 { get; set; }

        [JsonProperty("165")]
        public string _165 { get; set; }

        [JsonProperty("174")]
        public string _174 { get; set; }

        [JsonProperty("175")]
        public string _175 { get; set; }

        [JsonProperty("8")]
        public string _8 { get; set; }

        [JsonProperty("171")]
        public string _171 { get; set; }

        [JsonProperty("172")]
        public string _172 { get; set; }

        [JsonProperty("170")]
        public string _170 { get; set; }

        [JsonProperty("173")]
        public string _173 { get; set; }

        [JsonProperty("9")]
        public string _9 { get; set; }

        [JsonProperty("40")]
        public string _40 { get; set; }

        [JsonProperty("41")]
        public string _41 { get; set; }

        [JsonProperty("42")]
        public string _42 { get; set; }

        [JsonProperty("43")]
        public string _43 { get; set; }

        [JsonProperty("127")]
        public string _127 { get; set; }

        [JsonProperty("177")]
        public string _177 { get; set; }

        [JsonProperty("178")]
        public string _178 { get; set; }

        [JsonProperty("179")]
        public string _179 { get; set; }

        [JsonProperty("180")]
        public string _180 { get; set; }

        [JsonProperty("181")]
        public string _181 { get; set; }

        [JsonProperty("182")]
        public string _182 { get; set; }

        [JsonProperty("39")]
        public string _39 { get; set; }

        [JsonProperty("169")]
        public string _169 { get; set; }

        [JsonProperty("28")]
        public string _28 { get; set; }

        [JsonProperty("157")]
        public string _157 { get; set; }

        [JsonProperty("47")]
        public string _47 { get; set; }

        [JsonProperty("17")]
        public string _17 { get; set; }

        [JsonProperty("130")]
        public string _130 { get; set; }

        [JsonProperty("33")]
        public string _33 { get; set; }

        [JsonProperty("161")]
        public string _161 { get; set; }

        [JsonProperty("22")]
        public string _22 { get; set; }

        [JsonProperty("167")]
        public string _167 { get; set; }

        [JsonProperty("149")]
        public string _149 { get; set; }

        [JsonProperty("151")]
        public string _151 { get; set; }

        [JsonProperty("166")]
        public string _166 { get; set; }

        [JsonProperty("20")]
        public string _20 { get; set; }

        [JsonProperty("15")]
        public string _15 { get; set; }

        [JsonProperty("164")]
        public string _164 { get; set; }

        [JsonProperty("13")]
        public string _13 { get; set; }

        [JsonProperty("31")]
        public string _31 { get; set; }

        [JsonProperty("158")]
        public string _158 { get; set; }

        [JsonProperty("134")]
        public string _134 { get; set; }

        [JsonProperty("19")]
        public string _19 { get; set; }

        [JsonProperty("133")]
        public string _133 { get; set; }

        [JsonProperty("156")]
        public string _156 { get; set; }

        [JsonProperty("18")]
        public string _18 { get; set; }

        [JsonProperty("51")]
        public string _51 { get; set; }

        [JsonProperty("162")]
        public string _162 { get; set; }
    }

    public class Characteralex
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterbat
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Characterblaze
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Charactercavespider
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterchicken
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Charactercow
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Charactercreeper
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Characterdonkey
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterelderguardian
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterenderdragon
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Characterenderman
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Characterendermite
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Characterghast
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Characterguardian
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterhorse
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterhuman
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterirongolem
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Charactermagmacube
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Charactermooshroom
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterocelot
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterpig
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterrabbit
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Charactersheep
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Charactershulker
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Charactersilverfish
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Characterskeleton
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterslime
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Charactersnowman
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterspider
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Charactersquid
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Charactervillager
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterwitch
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Characterwither
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterwitherskeleton
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterwolf
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterzombie
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Characterzombiepigman
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Characterzombievillager
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Data
    {
        [JsonProperty("0")]
        public string _0 { get; set; }

        [JsonProperty("1")]
        public string _1 { get; set; }

        [JsonProperty("2")]
        public string _2 { get; set; }

        [JsonProperty("3")]
        public string _3 { get; set; }

        [JsonProperty("4")]
        public string _4 { get; set; }

        [JsonProperty("5")]
        public string _5 { get; set; }

        [JsonProperty("6")]
        public string _6 { get; set; }

        [JsonProperty("0x1+0x2")]
        public _0x10x2 _0x10x2 { get; set; }

        [JsonProperty("0x4+0x8")]
        public _0x40x8 _0x40x8 { get; set; }

        [JsonProperty("0x1+0x2+0x4")]
        public _0x10x20x4 _0x10x20x4 { get; set; }

        [JsonProperty("7")]
        public string _7 { get; set; }

        [JsonProperty("8")]
        public string _8 { get; set; }

        [JsonProperty("9")]
        public string _9 { get; set; }

        [JsonProperty("10")]
        public string _10 { get; set; }

        [JsonProperty("11")]
        public string _11 { get; set; }

        [JsonProperty("12")]
        public string _12 { get; set; }

        [JsonProperty("13")]
        public string _13 { get; set; }

        [JsonProperty("14")]
        public string _14 { get; set; }

        [JsonProperty("15")]
        public string _15 { get; set; }

        [JsonProperty("0x4")]
        public _0x4 _0x4 { get; set; }

        [JsonProperty("0x8")]
        public _0x8 _0x8 { get; set; }

        [JsonProperty("0x1")]
        public _0x1 _0x1 { get; set; }

        [JsonProperty("0x2")]
        public _0x2 _0x2 { get; set; }
    }

    public class Grass
    {
        [JsonProperty("type")]
        public Type Type { get; set; }
    }

    public class Human
    {
        [JsonProperty("variant")]
        public Variant Variant { get; set; }
    }

    public class LegacyBiomeIds
    {
        [JsonProperty("0")]
        public string _0 { get; set; }

        [JsonProperty("1")]
        public string _1 { get; set; }

        [JsonProperty("2")]
        public string _2 { get; set; }

        [JsonProperty("3")]
        public string _3 { get; set; }

        [JsonProperty("4")]
        public string _4 { get; set; }

        [JsonProperty("5")]
        public string _5 { get; set; }

        [JsonProperty("6")]
        public string _6 { get; set; }

        [JsonProperty("7")]
        public string _7 { get; set; }

        [JsonProperty("8")]
        public string _8 { get; set; }

        [JsonProperty("9")]
        public string _9 { get; set; }

        [JsonProperty("10")]
        public string _10 { get; set; }

        [JsonProperty("11")]
        public string _11 { get; set; }

        [JsonProperty("12")]
        public string _12 { get; set; }

        [JsonProperty("13")]
        public string _13 { get; set; }

        [JsonProperty("14")]
        public string _14 { get; set; }

        [JsonProperty("15")]
        public string _15 { get; set; }

        [JsonProperty("16")]
        public string _16 { get; set; }

        [JsonProperty("17")]
        public string _17 { get; set; }

        [JsonProperty("18")]
        public string _18 { get; set; }

        [JsonProperty("19")]
        public string _19 { get; set; }

        [JsonProperty("20")]
        public string _20 { get; set; }

        [JsonProperty("21")]
        public string _21 { get; set; }

        [JsonProperty("22")]
        public string _22 { get; set; }

        [JsonProperty("23")]
        public string _23 { get; set; }

        [JsonProperty("24")]
        public string _24 { get; set; }

        [JsonProperty("25")]
        public string _25 { get; set; }

        [JsonProperty("26")]
        public string _26 { get; set; }

        [JsonProperty("27")]
        public string _27 { get; set; }

        [JsonProperty("28")]
        public string _28 { get; set; }

        [JsonProperty("29")]
        public string _29 { get; set; }

        [JsonProperty("30")]
        public string _30 { get; set; }

        [JsonProperty("31")]
        public string _31 { get; set; }

        [JsonProperty("32")]
        public string _32 { get; set; }

        [JsonProperty("33")]
        public string _33 { get; set; }

        [JsonProperty("34")]
        public string _34 { get; set; }

        [JsonProperty("35")]
        public string _35 { get; set; }

        [JsonProperty("36")]
        public string _36 { get; set; }

        [JsonProperty("37")]
        public string _37 { get; set; }

        [JsonProperty("38")]
        public string _38 { get; set; }

        [JsonProperty("39")]
        public string _39 { get; set; }

        [JsonProperty("127")]
        public string _127 { get; set; }

        [JsonProperty("129")]
        public string _129 { get; set; }

        [JsonProperty("130")]
        public string _130 { get; set; }

        [JsonProperty("131")]
        public string _131 { get; set; }

        [JsonProperty("132")]
        public string _132 { get; set; }

        [JsonProperty("133")]
        public string _133 { get; set; }

        [JsonProperty("134")]
        public string _134 { get; set; }

        [JsonProperty("140")]
        public string _140 { get; set; }

        [JsonProperty("149")]
        public string _149 { get; set; }

        [JsonProperty("151")]
        public string _151 { get; set; }

        [JsonProperty("155")]
        public string _155 { get; set; }

        [JsonProperty("156")]
        public string _156 { get; set; }

        [JsonProperty("157")]
        public string _157 { get; set; }

        [JsonProperty("158")]
        public string _158 { get; set; }

        [JsonProperty("160")]
        public string _160 { get; set; }

        [JsonProperty("161")]
        public string _161 { get; set; }

        [JsonProperty("162")]
        public string _162 { get; set; }

        [JsonProperty("163")]
        public string _163 { get; set; }

        [JsonProperty("164")]
        public string _164 { get; set; }

        [JsonProperty("165")]
        public string _165 { get; set; }

        [JsonProperty("166")]
        public string _166 { get; set; }

        [JsonProperty("167")]
        public string _167 { get; set; }
    }

    public class LegacyBiomes
    {
        [JsonProperty("foresthills")]
        public string Foresthills { get; set; }

        [JsonProperty("swampland")]
        public string Swampland { get; set; }

        [JsonProperty("deserthills")]
        public string Deserthills { get; set; }

        [JsonProperty("junglehills")]
        public string Junglehills { get; set; }

        [JsonProperty("mushroomisland")]
        public string Mushroomisland { get; set; }

        [JsonProperty("mushroomislandshore")]
        public string Mushroomislandshore { get; set; }

        [JsonProperty("extremehills")]
        public string Extremehills { get; set; }

        [JsonProperty("extremehillsedge")]
        public string Extremehillsedge { get; set; }

        [JsonProperty("taigahills")]
        public string Taigahills { get; set; }

        [JsonProperty("iceplains")]
        public string Iceplains { get; set; }

        [JsonProperty("icemountains")]
        public string Icemountains { get; set; }

        [JsonProperty("frozenocean")]
        public string Frozenocean { get; set; }

        [JsonProperty("frozenriver")]
        public string Frozenriver { get; set; }

        [JsonProperty("mesa")]
        public string Mesa { get; set; }

        [JsonProperty("theend")]
        public string Theend { get; set; }

        [JsonProperty("snowy_tundra")]
        public string SnowyTundra { get; set; }

        [JsonProperty("snowy_mountains")]
        public string SnowyMountains { get; set; }

        [JsonProperty("mountain_edge")]
        public string MountainEdge { get; set; }

        [JsonProperty("wooded_mountains")]
        public string WoodedMountains { get; set; }

        [JsonProperty("gravelly_mountains")]
        public string GravellyMountains { get; set; }

        [JsonProperty("modified_gravelly_hills")]
        public string ModifiedGravellyHills { get; set; }

        [JsonProperty("giant_tree_taiga")]
        public string GiantTreeTaiga { get; set; }

        [JsonProperty("giant_tree_taiga_hills")]
        public string GiantTreeTaigaHills { get; set; }

        [JsonProperty("giant_spruce_taiga")]
        public string GiantSpruceTaiga { get; set; }

        [JsonProperty("giant_spruce_taiga_hills")]
        public string GiantSpruceTaigaHills { get; set; }

        [JsonProperty("stone_shore")]
        public string StoneShore { get; set; }

        [JsonProperty("jungle_edge")]
        public string JungleEdge { get; set; }

        [JsonProperty("modified_jungle_edge")]
        public string ModifiedJungleEdge { get; set; }

        [JsonProperty("tall_birch_forest")]
        public string TallBirchForest { get; set; }

        [JsonProperty("tall_birch_hills")]
        public string TallBirchHills { get; set; }

        [JsonProperty("shattered_savanna")]
        public string ShatteredSavanna { get; set; }

        [JsonProperty("shattered_savanna_plateau")]
        public string ShatteredSavannaPlateau { get; set; }

        [JsonProperty("wooded_badlands_plateau")]
        public string WoodedBadlandsPlateau { get; set; }

        [JsonProperty("modified_wooded_badlands_plateau")]
        public string ModifiedWoodedBadlandsPlateau { get; set; }

        [JsonProperty("desert_hills")]
        public string DesertHills { get; set; }

        [JsonProperty("desert_lakes")]
        public string DesertLakes { get; set; }

        [JsonProperty("wooded_hills")]
        public string WoodedHills { get; set; }

        [JsonProperty("taiga_hills")]
        public string TaigaHills { get; set; }

        [JsonProperty("taiga_mountains")]
        public string TaigaMountains { get; set; }

        [JsonProperty("snowy_taiga_hills")]
        public string SnowyTaigaHills { get; set; }

        [JsonProperty("snowy_taiga_mountains")]
        public string SnowyTaigaMountains { get; set; }

        [JsonProperty("swamp_hills")]
        public string SwampHills { get; set; }

        [JsonProperty("mushroom_field_shore")]
        public string MushroomFieldShore { get; set; }

        [JsonProperty("jungle_hills")]
        public string JungleHills { get; set; }

        [JsonProperty("modified_jungle")]
        public string ModifiedJungle { get; set; }

        [JsonProperty("bamboo_jungle_hills")]
        public string BambooJungleHills { get; set; }

        [JsonProperty("birch_forest_hills")]
        public string BirchForestHills { get; set; }

        [JsonProperty("dark_forest_hills")]
        public string DarkForestHills { get; set; }

        [JsonProperty("badlands_plateau")]
        public string BadlandsPlateau { get; set; }

        [JsonProperty("modified_badlands_plateau")]
        public string ModifiedBadlandsPlateau { get; set; }

        [JsonProperty("deep_warm_ocean")]
        public string DeepWarmOcean { get; set; }
    }

    public class LegacyBlockId
    {
        [JsonProperty("1")]
        public _1 _1 { get; set; }

        [JsonProperty("2")]
        public _2 _2 { get; set; }

        [JsonProperty("3")]
        public _3 _3 { get; set; }

        [JsonProperty("4")]
        public _4 _4 { get; set; }

        [JsonProperty("5")]
        public _5 _5 { get; set; }

        [JsonProperty("6")]
        public _6 _6 { get; set; }

        [JsonProperty("7")]
        public _7 _7 { get; set; }

        [JsonProperty("8")]
        public _8 _8 { get; set; }

        [JsonProperty("9")]
        public _9 _9 { get; set; }

        [JsonProperty("10")]
        public _10 _10 { get; set; }

        [JsonProperty("11")]
        public _11 _11 { get; set; }

        [JsonProperty("12")]
        public _12 _12 { get; set; }

        [JsonProperty("13")]
        public _13 _13 { get; set; }

        [JsonProperty("14")]
        public _14 _14 { get; set; }

        [JsonProperty("15")]
        public _15 _15 { get; set; }

        [JsonProperty("16")]
        public _16 _16 { get; set; }

        [JsonProperty("17")]
        public _17 _17 { get; set; }

        [JsonProperty("18")]
        public _18 _18 { get; set; }

        [JsonProperty("19")]
        public _19 _19 { get; set; }

        [JsonProperty("20")]
        public _20 _20 { get; set; }

        [JsonProperty("21")]
        public _21 _21 { get; set; }

        [JsonProperty("22")]
        public _22 _22 { get; set; }

        [JsonProperty("23")]
        public _23 _23 { get; set; }

        [JsonProperty("24")]
        public _24 _24 { get; set; }

        [JsonProperty("25")]
        public _25 _25 { get; set; }

        [JsonProperty("26")]
        public _26 _26 { get; set; }

        [JsonProperty("27")]
        public _27 _27 { get; set; }

        [JsonProperty("28")]
        public _28 _28 { get; set; }

        [JsonProperty("29")]
        public _29 _29 { get; set; }

        [JsonProperty("30")]
        public _30 _30 { get; set; }

        [JsonProperty("31")]
        public _31 _31 { get; set; }

        [JsonProperty("32")]
        public _32 _32 { get; set; }

        [JsonProperty("33")]
        public _33 _33 { get; set; }

        [JsonProperty("35")]
        public _35 _35 { get; set; }

        [JsonProperty("37")]
        public _37 _37 { get; set; }

        [JsonProperty("38")]
        public _38 _38 { get; set; }

        [JsonProperty("39")]
        public _39 _39 { get; set; }

        [JsonProperty("40")]
        public _40 _40 { get; set; }

        [JsonProperty("41")]
        public _41 _41 { get; set; }

        [JsonProperty("42")]
        public _42 _42 { get; set; }

        [JsonProperty("43")]
        public _43 _43 { get; set; }

        [JsonProperty("44")]
        public _44 _44 { get; set; }

        [JsonProperty("45")]
        public _45 _45 { get; set; }

        [JsonProperty("46")]
        public _46 _46 { get; set; }

        [JsonProperty("47")]
        public _47 _47 { get; set; }

        [JsonProperty("48")]
        public _48 _48 { get; set; }

        [JsonProperty("49")]
        public _49 _49 { get; set; }

        [JsonProperty("50")]
        public _50 _50 { get; set; }

        [JsonProperty("51")]
        public _51 _51 { get; set; }

        [JsonProperty("52")]
        public _52 _52 { get; set; }

        [JsonProperty("53")]
        public _53 _53 { get; set; }

        [JsonProperty("54")]
        public _54 _54 { get; set; }

        [JsonProperty("55")]
        public _55 _55 { get; set; }

        [JsonProperty("56")]
        public _56 _56 { get; set; }

        [JsonProperty("57")]
        public _57 _57 { get; set; }

        [JsonProperty("58")]
        public _58 _58 { get; set; }

        [JsonProperty("59")]
        public _59 _59 { get; set; }

        [JsonProperty("60")]
        public _60 _60 { get; set; }

        [JsonProperty("61")]
        public _61 _61 { get; set; }

        [JsonProperty("62")]
        public _62 _62 { get; set; }

        [JsonProperty("63")]
        public _63 _63 { get; set; }

        [JsonProperty("64")]
        public _64 _64 { get; set; }

        [JsonProperty("65")]
        public _65 _65 { get; set; }

        [JsonProperty("66")]
        public _66 _66 { get; set; }

        [JsonProperty("67")]
        public _67 _67 { get; set; }

        [JsonProperty("68")]
        public _68 _68 { get; set; }

        [JsonProperty("69")]
        public _69 _69 { get; set; }

        [JsonProperty("70")]
        public _70 _70 { get; set; }

        [JsonProperty("71")]
        public _71 _71 { get; set; }

        [JsonProperty("72")]
        public _72 _72 { get; set; }

        [JsonProperty("73")]
        public _73 _73 { get; set; }

        [JsonProperty("74")]
        public _74 _74 { get; set; }

        [JsonProperty("75")]
        public _75 _75 { get; set; }

        [JsonProperty("76")]
        public _76 _76 { get; set; }

        [JsonProperty("77")]
        public _77 _77 { get; set; }

        [JsonProperty("78")]
        public _78 _78 { get; set; }

        [JsonProperty("79")]
        public _79 _79 { get; set; }

        [JsonProperty("80")]
        public _80 _80 { get; set; }

        [JsonProperty("81")]
        public _81 _81 { get; set; }

        [JsonProperty("82")]
        public _82 _82 { get; set; }

        [JsonProperty("83")]
        public _83 _83 { get; set; }

        [JsonProperty("84")]
        public _84 _84 { get; set; }

        [JsonProperty("85")]
        public _85 _85 { get; set; }

        [JsonProperty("86")]
        public _86 _86 { get; set; }

        [JsonProperty("87")]
        public _87 _87 { get; set; }

        [JsonProperty("88")]
        public _88 _88 { get; set; }

        [JsonProperty("89")]
        public _89 _89 { get; set; }

        [JsonProperty("90")]
        public _90 _90 { get; set; }

        [JsonProperty("91")]
        public _91 _91 { get; set; }

        [JsonProperty("92")]
        public _92 _92 { get; set; }

        [JsonProperty("93")]
        public _93 _93 { get; set; }

        [JsonProperty("94")]
        public _94 _94 { get; set; }

        [JsonProperty("95")]
        public _95 _95 { get; set; }

        [JsonProperty("96")]
        public _96 _96 { get; set; }

        [JsonProperty("97")]
        public _97 _97 { get; set; }

        [JsonProperty("98")]
        public _98 _98 { get; set; }

        [JsonProperty("99")]
        public _99 _99 { get; set; }

        [JsonProperty("100")]
        public _100 _100 { get; set; }

        [JsonProperty("101")]
        public _101 _101 { get; set; }

        [JsonProperty("102")]
        public _102 _102 { get; set; }

        [JsonProperty("103")]
        public _103 _103 { get; set; }

        [JsonProperty("104")]
        public _104 _104 { get; set; }

        [JsonProperty("105")]
        public _105 _105 { get; set; }

        [JsonProperty("106")]
        public _106 _106 { get; set; }

        [JsonProperty("107")]
        public _107 _107 { get; set; }

        [JsonProperty("108")]
        public _108 _108 { get; set; }

        [JsonProperty("109")]
        public _109 _109 { get; set; }

        [JsonProperty("110")]
        public _110 _110 { get; set; }

        [JsonProperty("111")]
        public _111 _111 { get; set; }

        [JsonProperty("112")]
        public _112 _112 { get; set; }

        [JsonProperty("113")]
        public _113 _113 { get; set; }

        [JsonProperty("114")]
        public _114 _114 { get; set; }

        [JsonProperty("115")]
        public _115 _115 { get; set; }

        [JsonProperty("116")]
        public _116 _116 { get; set; }

        [JsonProperty("117")]
        public _117 _117 { get; set; }

        [JsonProperty("118")]
        public _118 _118 { get; set; }

        [JsonProperty("120")]
        public _120 _120 { get; set; }

        [JsonProperty("121")]
        public _121 _121 { get; set; }

        [JsonProperty("122")]
        public _122 _122 { get; set; }

        [JsonProperty("123")]
        public _123 _123 { get; set; }

        [JsonProperty("124")]
        public _124 _124 { get; set; }

        [JsonProperty("125")]
        public _125 _125 { get; set; }

        [JsonProperty("126")]
        public _126 _126 { get; set; }

        [JsonProperty("127")]
        public _127 _127 { get; set; }

        [JsonProperty("128")]
        public _128 _128 { get; set; }

        [JsonProperty("129")]
        public _129 _129 { get; set; }

        [JsonProperty("130")]
        public _130 _130 { get; set; }

        [JsonProperty("131")]
        public _131 _131 { get; set; }

        [JsonProperty("132")]
        public _132 _132 { get; set; }

        [JsonProperty("133")]
        public _133 _133 { get; set; }

        [JsonProperty("134")]
        public _134 _134 { get; set; }

        [JsonProperty("135")]
        public _135 _135 { get; set; }

        [JsonProperty("136")]
        public _136 _136 { get; set; }

        [JsonProperty("137")]
        public _137 _137 { get; set; }

        [JsonProperty("138")]
        public _138 _138 { get; set; }

        [JsonProperty("139")]
        public _139 _139 { get; set; }

        [JsonProperty("140")]
        public _140 _140 { get; set; }

        [JsonProperty("141")]
        public _141 _141 { get; set; }

        [JsonProperty("142")]
        public _142 _142 { get; set; }

        [JsonProperty("143")]
        public _143 _143 { get; set; }

        [JsonProperty("145")]
        public _145 _145 { get; set; }

        [JsonProperty("146")]
        public _146 _146 { get; set; }

        [JsonProperty("147")]
        public _147 _147 { get; set; }

        [JsonProperty("148")]
        public _148 _148 { get; set; }

        [JsonProperty("149")]
        public _149 _149 { get; set; }

        [JsonProperty("150")]
        public _150 _150 { get; set; }

        [JsonProperty("151")]
        public _151 _151 { get; set; }

        [JsonProperty("152")]
        public _152 _152 { get; set; }

        [JsonProperty("153")]
        public _153 _153 { get; set; }

        [JsonProperty("154")]
        public _154 _154 { get; set; }

        [JsonProperty("155")]
        public _155 _155 { get; set; }

        [JsonProperty("156")]
        public _156 _156 { get; set; }

        [JsonProperty("157")]
        public _157 _157 { get; set; }

        [JsonProperty("158")]
        public _158 _158 { get; set; }

        [JsonProperty("159")]
        public _159 _159 { get; set; }

        [JsonProperty("160")]
        public _160 _160 { get; set; }

        [JsonProperty("161")]
        public _161 _161 { get; set; }

        [JsonProperty("162")]
        public _162 _162 { get; set; }

        [JsonProperty("163")]
        public _163 _163 { get; set; }

        [JsonProperty("164")]
        public _164 _164 { get; set; }

        [JsonProperty("165")]
        public _165 _165 { get; set; }

        [JsonProperty("167")]
        public _167 _167 { get; set; }

        [JsonProperty("168")]
        public _168 _168 { get; set; }

        [JsonProperty("169")]
        public _169 _169 { get; set; }

        [JsonProperty("170")]
        public _170 _170 { get; set; }

        [JsonProperty("171")]
        public _171 _171 { get; set; }

        [JsonProperty("172")]
        public _172 _172 { get; set; }

        [JsonProperty("173")]
        public _173 _173 { get; set; }

        [JsonProperty("174")]
        public _174 _174 { get; set; }

        [JsonProperty("175")]
        public _175 _175 { get; set; }

        [JsonProperty("176")]
        public _176 _176 { get; set; }

        [JsonProperty("177")]
        public _177 _177 { get; set; }

        [JsonProperty("178")]
        public _178 _178 { get; set; }

        [JsonProperty("179")]
        public _179 _179 { get; set; }

        [JsonProperty("180")]
        public _180 _180 { get; set; }

        [JsonProperty("181")]
        public _181 _181 { get; set; }

        [JsonProperty("182")]
        public _182 _182 { get; set; }

        [JsonProperty("183")]
        public _183 _183 { get; set; }

        [JsonProperty("184")]
        public _184 _184 { get; set; }

        [JsonProperty("185")]
        public _185 _185 { get; set; }

        [JsonProperty("186")]
        public _186 _186 { get; set; }

        [JsonProperty("187")]
        public _187 _187 { get; set; }

        [JsonProperty("188")]
        public _188 _188 { get; set; }

        [JsonProperty("189")]
        public _189 _189 { get; set; }

        [JsonProperty("190")]
        public _190 _190 { get; set; }

        [JsonProperty("191")]
        public _191 _191 { get; set; }

        [JsonProperty("192")]
        public _192 _192 { get; set; }

        [JsonProperty("193")]
        public _193 _193 { get; set; }

        [JsonProperty("194")]
        public _194 _194 { get; set; }

        [JsonProperty("195")]
        public _195 _195 { get; set; }

        [JsonProperty("196")]
        public _196 _196 { get; set; }

        [JsonProperty("197")]
        public _197 _197 { get; set; }

        [JsonProperty("198")]
        public _198 _198 { get; set; }

        [JsonProperty("199")]
        public _199 _199 { get; set; }

        [JsonProperty("200")]
        public _200 _200 { get; set; }

        [JsonProperty("201")]
        public _201 _201 { get; set; }

        [JsonProperty("202")]
        public _202 _202 { get; set; }

        [JsonProperty("203")]
        public _203 _203 { get; set; }

        [JsonProperty("204")]
        public _204 _204 { get; set; }

        [JsonProperty("205")]
        public _205 _205 { get; set; }

        [JsonProperty("206")]
        public _206 _206 { get; set; }

        [JsonProperty("207")]
        public _207 _207 { get; set; }

        [JsonProperty("208")]
        public _208 _208 { get; set; }

        [JsonProperty("210")]
        public _210 _210 { get; set; }

        [JsonProperty("211")]
        public _211 _211 { get; set; }

        [JsonProperty("212")]
        public _212 _212 { get; set; }

        [JsonProperty("213")]
        public _213 _213 { get; set; }

        [JsonProperty("214")]
        public _214 _214 { get; set; }

        [JsonProperty("215")]
        public _215 _215 { get; set; }

        [JsonProperty("216")]
        public _216 _216 { get; set; }

        [JsonProperty("218")]
        public _218 _218 { get; set; }

        [JsonProperty("219")]
        public _219 _219 { get; set; }

        [JsonProperty("220")]
        public _220 _220 { get; set; }

        [JsonProperty("221")]
        public _221 _221 { get; set; }

        [JsonProperty("222")]
        public _222 _222 { get; set; }

        [JsonProperty("223")]
        public _223 _223 { get; set; }

        [JsonProperty("224")]
        public _224 _224 { get; set; }

        [JsonProperty("225")]
        public _225 _225 { get; set; }

        [JsonProperty("226")]
        public _226 _226 { get; set; }

        [JsonProperty("227")]
        public _227 _227 { get; set; }

        [JsonProperty("228")]
        public _228 _228 { get; set; }

        [JsonProperty("229")]
        public _229 _229 { get; set; }

        [JsonProperty("230")]
        public _230 _230 { get; set; }

        [JsonProperty("231")]
        public _231 _231 { get; set; }

        [JsonProperty("232")]
        public _232 _232 { get; set; }

        [JsonProperty("233")]
        public _233 _233 { get; set; }

        [JsonProperty("234")]
        public _234 _234 { get; set; }

        [JsonProperty("235")]
        public _235 _235 { get; set; }

        [JsonProperty("236")]
        public _236 _236 { get; set; }

        [JsonProperty("237")]
        public _237 _237 { get; set; }

        [JsonProperty("238")]
        public _238 _238 { get; set; }

        [JsonProperty("239")]
        public _239 _239 { get; set; }

        [JsonProperty("240")]
        public _240 _240 { get; set; }

        [JsonProperty("241")]
        public _241 _241 { get; set; }

        [JsonProperty("242")]
        public _242 _242 { get; set; }

        [JsonProperty("243")]
        public _243 _243 { get; set; }

        [JsonProperty("244")]
        public _244 _244 { get; set; }

        [JsonProperty("245")]
        public _245 _245 { get; set; }

        [JsonProperty("246")]
        public _246 _246 { get; set; }

        [JsonProperty("247")]
        public _247 _247 { get; set; }

        [JsonProperty("248")]
        public _248 _248 { get; set; }

        [JsonProperty("249")]
        public _249 _249 { get; set; }

        [JsonProperty("250")]
        public _250 _250 { get; set; }

        [JsonProperty("251")]
        public _251 _251 { get; set; }

        [JsonProperty("252")]
        public _252 _252 { get; set; }

        [JsonProperty("255")]
        public _255 _255 { get; set; }
    }

    public class LegacyBlockNames
    {
        [JsonProperty("cobblestone_wall")]
        public string CobblestoneWall { get; set; }

        [JsonProperty("grass_path")]
        public string GrassPath { get; set; }

        [JsonProperty("target_block")]
        public string TargetBlock { get; set; }

        [JsonProperty("jigsaw_block")]
        public string JigsawBlock { get; set; }
    }

    public class LegacyBlockStates
    {
    }

    public class LegacyBlockStateValues
    {
        [JsonProperty("grass")]
        public Grass Grass { get; set; }
    }

    public class LegacyBlockTextureName
    {
        [JsonProperty("block/grass_block_top")]
        public string BlockGrassBlockTop { get; set; }

        [JsonProperty("block/grass_block_side_overlay")]
        public string BlockGrassBlockSideOverlay { get; set; }

        [JsonProperty("block/grass_block_side")]
        public string BlockGrassBlockSide { get; set; }

        [JsonProperty("block/grass_block_snow")]
        public string BlockGrassBlockSnow { get; set; }

        [JsonProperty("block/podzol_top")]
        public string BlockPodzolTop { get; set; }

        [JsonProperty("block/podzol_side")]
        public string BlockPodzolSide { get; set; }

        [JsonProperty("block/oak_planks")]
        public string BlockOakPlanks { get; set; }

        [JsonProperty("block/spruce_planks")]
        public string BlockSprucePlanks { get; set; }

        [JsonProperty("block/birch_planks")]
        public string BlockBirchPlanks { get; set; }

        [JsonProperty("block/jungle_planks")]
        public string BlockJunglePlanks { get; set; }

        [JsonProperty("block/acacia_planks")]
        public string BlockAcaciaPlanks { get; set; }

        [JsonProperty("block/dark_oak_planks")]
        public string BlockDarkOakPlanks { get; set; }

        [JsonProperty("block/oak_sapling")]
        public string BlockOakSapling { get; set; }

        [JsonProperty("block/spruce_sapling")]
        public string BlockSpruceSapling { get; set; }

        [JsonProperty("block/birch_sapling")]
        public string BlockBirchSapling { get; set; }

        [JsonProperty("block/jungle_sapling")]
        public string BlockJungleSapling { get; set; }

        [JsonProperty("block/acacia_sapling")]
        public string BlockAcaciaSapling { get; set; }

        [JsonProperty("block/dark_oak_sapling")]
        public string BlockDarkOakSapling { get; set; }

        [JsonProperty("block/oak_log_top")]
        public string BlockOakLogTop { get; set; }

        [JsonProperty("block/oak_log")]
        public string BlockOakLog { get; set; }

        [JsonProperty("block/spruce_log_top")]
        public string BlockSpruceLogTop { get; set; }

        [JsonProperty("block/spruce_log")]
        public string BlockSpruceLog { get; set; }

        [JsonProperty("block/birch_log_top")]
        public string BlockBirchLogTop { get; set; }

        [JsonProperty("block/birch_log")]
        public string BlockBirchLog { get; set; }

        [JsonProperty("block/jungle_log_top")]
        public string BlockJungleLogTop { get; set; }

        [JsonProperty("block/jungle_log")]
        public string BlockJungleLog { get; set; }

        [JsonProperty("block/acacia_log_top")]
        public string BlockAcaciaLogTop { get; set; }

        [JsonProperty("block/acacia_log")]
        public string BlockAcaciaLog { get; set; }

        [JsonProperty("block/dark_oak_log_top")]
        public string BlockDarkOakLogTop { get; set; }

        [JsonProperty("block/dark_oak_log")]
        public string BlockDarkOakLog { get; set; }

        [JsonProperty("block/mossy_cobblestone")]
        public string BlockMossyCobblestone { get; set; }

        [JsonProperty("block/oak_leaves")]
        public string BlockOakLeaves { get; set; }

        [JsonProperty("block/spruce_leaves")]
        public string BlockSpruceLeaves { get; set; }

        [JsonProperty("block/birch_leaves")]
        public string BlockBirchLeaves { get; set; }

        [JsonProperty("block/jungle_leaves")]
        public string BlockJungleLeaves { get; set; }

        [JsonProperty("block/acacia_leaves")]
        public string BlockAcaciaLeaves { get; set; }

        [JsonProperty("block/dark_oak_leaves")]
        public string BlockDarkOakLeaves { get; set; }

        [JsonProperty("block/stone_bricks")]
        public string BlockStoneBricks { get; set; }

        [JsonProperty("block/mossy_stone_bricks")]
        public string BlockMossyStoneBricks { get; set; }

        [JsonProperty("block/cracked_stone_bricks")]
        public string BlockCrackedStoneBricks { get; set; }

        [JsonProperty("block/chiseled_stone_bricks")]
        public string BlockChiseledStoneBricks { get; set; }

        [JsonProperty("block/mushroom_stem")]
        public string BlockMushroomStem { get; set; }

        [JsonProperty("block/brown_mushroom_block")]
        public string BlockBrownMushroomBlock { get; set; }

        [JsonProperty("block/red_mushroom_block")]
        public string BlockRedMushroomBlock { get; set; }

        [JsonProperty("block/white_terracotta")]
        public string BlockWhiteTerracotta { get; set; }

        [JsonProperty("block/orange_terracotta")]
        public string BlockOrangeTerracotta { get; set; }

        [JsonProperty("block/magenta_terracotta")]
        public string BlockMagentaTerracotta { get; set; }

        [JsonProperty("block/light_blue_terracotta")]
        public string BlockLightBlueTerracotta { get; set; }

        [JsonProperty("block/yellow_terracotta")]
        public string BlockYellowTerracotta { get; set; }

        [JsonProperty("block/lime_terracotta")]
        public string BlockLimeTerracotta { get; set; }

        [JsonProperty("block/pink_terracotta")]
        public string BlockPinkTerracotta { get; set; }

        [JsonProperty("block/gray_terracotta")]
        public string BlockGrayTerracotta { get; set; }

        [JsonProperty("block/light_gray_terracotta")]
        public string BlockLightGrayTerracotta { get; set; }

        [JsonProperty("block/cyan_terracotta")]
        public string BlockCyanTerracotta { get; set; }

        [JsonProperty("block/purple_terracotta")]
        public string BlockPurpleTerracotta { get; set; }

        [JsonProperty("block/blue_terracotta")]
        public string BlockBlueTerracotta { get; set; }

        [JsonProperty("block/brown_terracotta")]
        public string BlockBrownTerracotta { get; set; }

        [JsonProperty("block/green_terracotta")]
        public string BlockGreenTerracotta { get; set; }

        [JsonProperty("block/red_terracotta")]
        public string BlockRedTerracotta { get; set; }

        [JsonProperty("block/black_terracotta")]
        public string BlockBlackTerracotta { get; set; }

        [JsonProperty("block/packed_ice")]
        public string BlockPackedIce { get; set; }

        [JsonProperty("block/sandstone")]
        public string BlockSandstone { get; set; }

        [JsonProperty("block/chiseled_sandstone")]
        public string BlockChiseledSandstone { get; set; }

        [JsonProperty("block/cut_sandstone")]
        public string BlockCutSandstone { get; set; }

        [JsonProperty("block/chiseled_quartz_block_top")]
        public string BlockChiseledQuartzBlockTop { get; set; }

        [JsonProperty("block/chiseled_quartz_block")]
        public string BlockChiseledQuartzBlock { get; set; }

        [JsonProperty("block/quartz_pillar_top")]
        public string BlockQuartzPillarTop { get; set; }

        [JsonProperty("block/quartz_pillar")]
        public string BlockQuartzPillar { get; set; }

        [JsonProperty("block/end_portal_frame_eye")]
        public string BlockEndPortalFrameEye { get; set; }

        [JsonProperty("block/end_portal_frame_top")]
        public string BlockEndPortalFrameTop { get; set; }

        [JsonProperty("block/end_portal_frame_side")]
        public string BlockEndPortalFrameSide { get; set; }

        [JsonProperty("block/anvil")]
        public string BlockAnvil { get; set; }

        [JsonProperty("block/anvil_top")]
        public string BlockAnvilTop { get; set; }

        [JsonProperty("block/chipped_anvil_top")]
        public string BlockChippedAnvilTop { get; set; }

        [JsonProperty("block/damaged_anvil_top")]
        public string BlockDamagedAnvilTop { get; set; }

        [JsonProperty("block/torch")]
        public string BlockTorch { get; set; }

        [JsonProperty("block/furnace_front")]
        public string BlockFurnaceFront { get; set; }

        [JsonProperty("block/dispenser_front")]
        public string BlockDispenserFront { get; set; }

        [JsonProperty("block/dropper_front")]
        public string BlockDropperFront { get; set; }

        [JsonProperty("block/oak_door_top")]
        public string BlockOakDoorTop { get; set; }

        [JsonProperty("block/oak_door_bottom")]
        public string BlockOakDoorBottom { get; set; }

        [JsonProperty("block/iron_door_top")]
        public string BlockIronDoorTop { get; set; }

        [JsonProperty("block/iron_door_bottom")]
        public string BlockIronDoorBottom { get; set; }

        [JsonProperty("block/oak_trapdoor")]
        public string BlockOakTrapdoor { get; set; }

        [JsonProperty("block/white_stained_glass_pane_top")]
        public string BlockWhiteStainedGlassPaneTop { get; set; }

        [JsonProperty("block/orange_stained_glass_pane_top")]
        public string BlockOrangeStainedGlassPaneTop { get; set; }

        [JsonProperty("block/magenta_stained_glass_pane_top")]
        public string BlockMagentaStainedGlassPaneTop { get; set; }

        [JsonProperty("block/light_blue_stained_glass_pane_top")]
        public string BlockLightBlueStainedGlassPaneTop { get; set; }

        [JsonProperty("block/yellow_stained_glass_pane_top")]
        public string BlockYellowStainedGlassPaneTop { get; set; }

        [JsonProperty("block/lime_stained_glass_pane_top")]
        public string BlockLimeStainedGlassPaneTop { get; set; }

        [JsonProperty("block/pink_stained_glass_pane_top")]
        public string BlockPinkStainedGlassPaneTop { get; set; }

        [JsonProperty("block/gray_stained_glass_pane_top")]
        public string BlockGrayStainedGlassPaneTop { get; set; }

        [JsonProperty("block/light_gray_stained_glass_pane_top")]
        public string BlockLightGrayStainedGlassPaneTop { get; set; }

        [JsonProperty("block/cyan_stained_glass_pane_top")]
        public string BlockCyanStainedGlassPaneTop { get; set; }

        [JsonProperty("block/purple_stained_glass_pane_top")]
        public string BlockPurpleStainedGlassPaneTop { get; set; }

        [JsonProperty("block/blue_stained_glass_pane_top")]
        public string BlockBlueStainedGlassPaneTop { get; set; }

        [JsonProperty("block/brown_stained_glass_pane_top")]
        public string BlockBrownStainedGlassPaneTop { get; set; }

        [JsonProperty("block/green_stained_glass_pane_top")]
        public string BlockGreenStainedGlassPaneTop { get; set; }

        [JsonProperty("block/red_stained_glass_pane_top")]
        public string BlockRedStainedGlassPaneTop { get; set; }

        [JsonProperty("block/black_stained_glass_pane_top")]
        public string BlockBlackStainedGlassPaneTop { get; set; }

        [JsonProperty("block/white_stained_glass")]
        public string BlockWhiteStainedGlass { get; set; }

        [JsonProperty("block/orange_stained_glass")]
        public string BlockOrangeStainedGlass { get; set; }

        [JsonProperty("block/magenta_stained_glass")]
        public string BlockMagentaStainedGlass { get; set; }

        [JsonProperty("block/light_blue_stained_glass")]
        public string BlockLightBlueStainedGlass { get; set; }

        [JsonProperty("block/yellow_stained_glass")]
        public string BlockYellowStainedGlass { get; set; }

        [JsonProperty("block/lime_stained_glass")]
        public string BlockLimeStainedGlass { get; set; }

        [JsonProperty("block/pink_stained_glass")]
        public string BlockPinkStainedGlass { get; set; }

        [JsonProperty("block/gray_stained_glass")]
        public string BlockGrayStainedGlass { get; set; }

        [JsonProperty("block/light_gray_stained_glass")]
        public string BlockLightGrayStainedGlass { get; set; }

        [JsonProperty("block/cyan_stained_glass")]
        public string BlockCyanStainedGlass { get; set; }

        [JsonProperty("block/purple_stained_glass")]
        public string BlockPurpleStainedGlass { get; set; }

        [JsonProperty("block/blue_stained_glass")]
        public string BlockBlueStainedGlass { get; set; }

        [JsonProperty("block/brown_stained_glass")]
        public string BlockBrownStainedGlass { get; set; }

        [JsonProperty("block/green_stained_glass")]
        public string BlockGreenStainedGlass { get; set; }

        [JsonProperty("block/red_stained_glass")]
        public string BlockRedStainedGlass { get; set; }

        [JsonProperty("block/black_stained_glass")]
        public string BlockBlackStainedGlass { get; set; }

        [JsonProperty("block/farmland")]
        public string BlockFarmland { get; set; }

        [JsonProperty("block/wheat_stage0")]
        public string BlockWheatStage0 { get; set; }

        [JsonProperty("block/wheat_stage1")]
        public string BlockWheatStage1 { get; set; }

        [JsonProperty("block/wheat_stage2")]
        public string BlockWheatStage2 { get; set; }

        [JsonProperty("block/wheat_stage3")]
        public string BlockWheatStage3 { get; set; }

        [JsonProperty("block/wheat_stage4")]
        public string BlockWheatStage4 { get; set; }

        [JsonProperty("block/wheat_stage5")]
        public string BlockWheatStage5 { get; set; }

        [JsonProperty("block/wheat_stage6")]
        public string BlockWheatStage6 { get; set; }

        [JsonProperty("block/wheat_stage7")]
        public string BlockWheatStage7 { get; set; }

        [JsonProperty("block/carrots_stage0")]
        public string BlockCarrotsStage0 { get; set; }

        [JsonProperty("block/carrots_stage1")]
        public string BlockCarrotsStage1 { get; set; }

        [JsonProperty("block/carrots_stage2")]
        public string BlockCarrotsStage2 { get; set; }

        [JsonProperty("block/carrots_stage3")]
        public string BlockCarrotsStage3 { get; set; }

        [JsonProperty("block/potatoes_stage0")]
        public string BlockPotatoesStage0 { get; set; }

        [JsonProperty("block/potatoes_stage1")]
        public string BlockPotatoesStage1 { get; set; }

        [JsonProperty("block/potatoes_stage2")]
        public string BlockPotatoesStage2 { get; set; }

        [JsonProperty("block/potatoes_stage3")]
        public string BlockPotatoesStage3 { get; set; }

        [JsonProperty("block/nether_wart_stage0")]
        public string BlockNetherWartStage0 { get; set; }

        [JsonProperty("block/nether_wart_stage1")]
        public string BlockNetherWartStage1 { get; set; }

        [JsonProperty("block/nether_wart_stage2")]
        public string BlockNetherWartStage2 { get; set; }

        [JsonProperty("block/cocoa_stage0")]
        public string BlockCocoaStage0 { get; set; }

        [JsonProperty("block/cocoa_stage1")]
        public string BlockCocoaStage1 { get; set; }

        [JsonProperty("block/cocoa_stage2")]
        public string BlockCocoaStage2 { get; set; }

        [JsonProperty("block/pumpkin_stem")]
        public string BlockPumpkinStem { get; set; }

        [JsonProperty("block/attached_pumpkin_stem")]
        public string BlockAttachedPumpkinStem { get; set; }

        [JsonProperty("block/melon_stem")]
        public string BlockMelonStem { get; set; }

        [JsonProperty("block/attached_melon_stem")]
        public string BlockAttachedMelonStem { get; set; }

        [JsonProperty("block/lily_pad")]
        public string BlockLilyPad { get; set; }

        [JsonProperty("block/sugar_cane")]
        public string BlockSugarCane { get; set; }

        [JsonProperty("block/cobweb")]
        public string BlockCobweb { get; set; }

        [JsonProperty("block/short_grass")]
        public string BlockShortGrass { get; set; }

        [JsonProperty("block/dead_bush")]
        public string BlockDeadBush { get; set; }

        [JsonProperty("block/brown_mushroom")]
        public string BlockBrownMushroom { get; set; }

        [JsonProperty("block/red_mushroom")]
        public string BlockRedMushroom { get; set; }

        [JsonProperty("block/dandelion")]
        public string BlockDandelion { get; set; }

        [JsonProperty("block/poppy")]
        public string BlockPoppy { get; set; }

        [JsonProperty("block/blue_orchid")]
        public string BlockBlueOrchid { get; set; }

        [JsonProperty("block/allium")]
        public string BlockAllium { get; set; }

        [JsonProperty("block/azure_bluet")]
        public string BlockAzureBluet { get; set; }

        [JsonProperty("block/red_tulip")]
        public string BlockRedTulip { get; set; }

        [JsonProperty("block/orange_tulip")]
        public string BlockOrangeTulip { get; set; }

        [JsonProperty("block/white_tulip")]
        public string BlockWhiteTulip { get; set; }

        [JsonProperty("block/pink_tulip")]
        public string BlockPinkTulip { get; set; }

        [JsonProperty("block/oxeye_daisy")]
        public string BlockOxeyeDaisy { get; set; }

        [JsonProperty("block/sunflower_bottom")]
        public string BlockSunflowerBottom { get; set; }

        [JsonProperty("block/sunflower_top")]
        public string BlockSunflowerTop { get; set; }

        [JsonProperty("block/sunflower_front")]
        public string BlockSunflowerFront { get; set; }

        [JsonProperty("block/sunflower_back")]
        public string BlockSunflowerBack { get; set; }

        [JsonProperty("block/lilac_bottom")]
        public string BlockLilacBottom { get; set; }

        [JsonProperty("block/lilac_top")]
        public string BlockLilacTop { get; set; }

        [JsonProperty("block/tall_grass_bottom")]
        public string BlockTallGrassBottom { get; set; }

        [JsonProperty("block/tall_grass_top")]
        public string BlockTallGrassTop { get; set; }

        [JsonProperty("block/large_fern_bottom")]
        public string BlockLargeFernBottom { get; set; }

        [JsonProperty("block/large_fern_top")]
        public string BlockLargeFernTop { get; set; }

        [JsonProperty("block/rose_bush_bottom")]
        public string BlockRoseBushBottom { get; set; }

        [JsonProperty("block/rose_bush_top")]
        public string BlockRoseBushTop { get; set; }

        [JsonProperty("block/peony_bottom")]
        public string BlockPeonyBottom { get; set; }

        [JsonProperty("block/peony_top")]
        public string BlockPeonyTop { get; set; }

        [JsonProperty("block/white_wool")]
        public string BlockWhiteWool { get; set; }

        [JsonProperty("block/orange_wool")]
        public string BlockOrangeWool { get; set; }

        [JsonProperty("block/magenta_wool")]
        public string BlockMagentaWool { get; set; }

        [JsonProperty("block/light_blue_wool")]
        public string BlockLightBlueWool { get; set; }

        [JsonProperty("block/yellow_wool")]
        public string BlockYellowWool { get; set; }

        [JsonProperty("block/lime_wool")]
        public string BlockLimeWool { get; set; }

        [JsonProperty("block/pink_wool")]
        public string BlockPinkWool { get; set; }

        [JsonProperty("block/gray_wool")]
        public string BlockGrayWool { get; set; }

        [JsonProperty("block/light_gray_wool")]
        public string BlockLightGrayWool { get; set; }

        [JsonProperty("block/cyan_wool")]
        public string BlockCyanWool { get; set; }

        [JsonProperty("block/purple_wool")]
        public string BlockPurpleWool { get; set; }

        [JsonProperty("block/blue_wool")]
        public string BlockBlueWool { get; set; }

        [JsonProperty("block/brown_wool")]
        public string BlockBrownWool { get; set; }

        [JsonProperty("block/green_wool")]
        public string BlockGreenWool { get; set; }

        [JsonProperty("block/red_wool")]
        public string BlockRedWool { get; set; }

        [JsonProperty("block/black_wool")]
        public string BlockBlackWool { get; set; }

        [JsonProperty("block/note_block")]
        public string BlockNoteBlock { get; set; }

        [JsonProperty("block/rail")]
        public string BlockRail { get; set; }

        [JsonProperty("block/rail_corner")]
        public string BlockRailCorner { get; set; }

        [JsonProperty("block/powered_rail")]
        public string BlockPoweredRail { get; set; }

        [JsonProperty("block/powered_rail_on")]
        public string BlockPoweredRailOn { get; set; }

        [JsonProperty("block/detector_rail")]
        public string BlockDetectorRail { get; set; }

        [JsonProperty("block/detector_rail_on")]
        public string BlockDetectorRailOn { get; set; }

        [JsonProperty("block/activator_rail")]
        public string BlockActivatorRail { get; set; }

        [JsonProperty("block/activator_rail_on")]
        public string BlockActivatorRailOn { get; set; }

        [JsonProperty("block/redstone_torch")]
        public string BlockRedstoneTorch { get; set; }

        [JsonProperty("block/repeater")]
        public string BlockRepeater { get; set; }

        [JsonProperty("block/comparator")]
        public string BlockComparator { get; set; }

        [JsonProperty("block/piston_top")]
        public string BlockPistonTop { get; set; }

        [JsonProperty("block/tripwire_hook")]
        public string BlockTripwireHook { get; set; }

        [JsonProperty("block/polished_granite")]
        public string BlockPolishedGranite { get; set; }

        [JsonProperty("block/diorite")]
        public string BlockDiorite { get; set; }

        [JsonProperty("block/polished_diorite")]
        public string BlockPolishedDiorite { get; set; }

        [JsonProperty("block/andesite")]
        public string BlockAndesite { get; set; }

        [JsonProperty("block/polished_andesite")]
        public string BlockPolishedAndesite { get; set; }

        [JsonProperty("block/wet_sponge")]
        public string BlockWetSponge { get; set; }

        [JsonProperty("block/slime_block")]
        public string BlockSlimeBlock { get; set; }

        [JsonProperty("block/spruce_door_top")]
        public string BlockSpruceDoorTop { get; set; }

        [JsonProperty("block/spruce_door_bottom")]
        public string BlockSpruceDoorBottom { get; set; }

        [JsonProperty("block/birch_door_top")]
        public string BlockBirchDoorTop { get; set; }

        [JsonProperty("block/birch_door_bottom")]
        public string BlockBirchDoorBottom { get; set; }

        [JsonProperty("block/jungle_door_top")]
        public string BlockJungleDoorTop { get; set; }

        [JsonProperty("block/jungle_door_bottom")]
        public string BlockJungleDoorBottom { get; set; }

        [JsonProperty("block/acacia_door_top")]
        public string BlockAcaciaDoorTop { get; set; }

        [JsonProperty("block/acacia_door_bottom")]
        public string BlockAcaciaDoorBottom { get; set; }

        [JsonProperty("block/dark_oak_door_top")]
        public string BlockDarkOakDoorTop { get; set; }

        [JsonProperty("block/dark_oak_door_bottom")]
        public string BlockDarkOakDoorBottom { get; set; }

        [JsonProperty("block/red_sandstone")]
        public string BlockRedSandstone { get; set; }

        [JsonProperty("block/chiseled_red_sandstone")]
        public string BlockChiseledRedSandstone { get; set; }

        [JsonProperty("block/cut_red_sandstone")]
        public string BlockCutRedSandstone { get; set; }

        [JsonProperty("block/prismarine")]
        public string BlockPrismarine { get; set; }

        [JsonProperty("block/dark_prismarine")]
        public string BlockDarkPrismarine { get; set; }

        [JsonProperty("block/end_stone_bricks")]
        public string BlockEndStoneBricks { get; set; }

        [JsonProperty("block/beetroots_stage0")]
        public string BlockBeetrootsStage0 { get; set; }

        [JsonProperty("block/beetroots_stage1")]
        public string BlockBeetrootsStage1 { get; set; }

        [JsonProperty("block/beetroots_stage2")]
        public string BlockBeetrootsStage2 { get; set; }

        [JsonProperty("block/beetroots_stage3")]
        public string BlockBeetrootsStage3 { get; set; }

        [JsonProperty("block/observer_back_on")]
        public string BlockObserverBackOn { get; set; }

        [JsonProperty("block/white_glazed_terracotta")]
        public string BlockWhiteGlazedTerracotta { get; set; }

        [JsonProperty("block/orange_glazed_terracotta")]
        public string BlockOrangeGlazedTerracotta { get; set; }

        [JsonProperty("block/magenta_glazed_terracotta")]
        public string BlockMagentaGlazedTerracotta { get; set; }

        [JsonProperty("block/light_blue_glazed_terracotta")]
        public string BlockLightBlueGlazedTerracotta { get; set; }

        [JsonProperty("block/yellow_glazed_terracotta")]
        public string BlockYellowGlazedTerracotta { get; set; }

        [JsonProperty("block/lime_glazed_terracotta")]
        public string BlockLimeGlazedTerracotta { get; set; }

        [JsonProperty("block/pink_glazed_terracotta")]
        public string BlockPinkGlazedTerracotta { get; set; }

        [JsonProperty("block/gray_glazed_terracotta")]
        public string BlockGrayGlazedTerracotta { get; set; }

        [JsonProperty("block/light_gray_glazed_terracotta")]
        public string BlockLightGrayGlazedTerracotta { get; set; }

        [JsonProperty("block/cyan_glazed_terracotta")]
        public string BlockCyanGlazedTerracotta { get; set; }

        [JsonProperty("block/purple_glazed_terracotta")]
        public string BlockPurpleGlazedTerracotta { get; set; }

        [JsonProperty("block/blue_glazed_terracotta")]
        public string BlockBlueGlazedTerracotta { get; set; }

        [JsonProperty("block/brown_glazed_terracotta")]
        public string BlockBrownGlazedTerracotta { get; set; }

        [JsonProperty("block/green_glazed_terracotta")]
        public string BlockGreenGlazedTerracotta { get; set; }

        [JsonProperty("block/red_glazed_terracotta")]
        public string BlockRedGlazedTerracotta { get; set; }

        [JsonProperty("block/black_glazed_terracotta")]
        public string BlockBlackGlazedTerracotta { get; set; }

        [JsonProperty("block/white_concrete")]
        public string BlockWhiteConcrete { get; set; }

        [JsonProperty("block/orange_concrete")]
        public string BlockOrangeConcrete { get; set; }

        [JsonProperty("block/magenta_concrete")]
        public string BlockMagentaConcrete { get; set; }

        [JsonProperty("block/light_blue_concrete")]
        public string BlockLightBlueConcrete { get; set; }

        [JsonProperty("block/yellow_concrete")]
        public string BlockYellowConcrete { get; set; }

        [JsonProperty("block/lime_concrete")]
        public string BlockLimeConcrete { get; set; }

        [JsonProperty("block/pink_concrete")]
        public string BlockPinkConcrete { get; set; }

        [JsonProperty("block/gray_concrete")]
        public string BlockGrayConcrete { get; set; }

        [JsonProperty("block/light_gray_concrete")]
        public string BlockLightGrayConcrete { get; set; }

        [JsonProperty("block/cyan_concrete")]
        public string BlockCyanConcrete { get; set; }

        [JsonProperty("block/purple_concrete")]
        public string BlockPurpleConcrete { get; set; }

        [JsonProperty("block/blue_concrete")]
        public string BlockBlueConcrete { get; set; }

        [JsonProperty("block/brown_concrete")]
        public string BlockBrownConcrete { get; set; }

        [JsonProperty("block/green_concrete")]
        public string BlockGreenConcrete { get; set; }

        [JsonProperty("block/red_concrete")]
        public string BlockRedConcrete { get; set; }

        [JsonProperty("block/black_concrete")]
        public string BlockBlackConcrete { get; set; }

        [JsonProperty("block/white_concrete_powder")]
        public string BlockWhiteConcretePowder { get; set; }

        [JsonProperty("block/orange_concrete_powder")]
        public string BlockOrangeConcretePowder { get; set; }

        [JsonProperty("block/magenta_concrete_powder")]
        public string BlockMagentaConcretePowder { get; set; }

        [JsonProperty("block/light_blue_concrete_powder")]
        public string BlockLightBlueConcretePowder { get; set; }

        [JsonProperty("block/yellow_concrete_powder")]
        public string BlockYellowConcretePowder { get; set; }

        [JsonProperty("block/lime_concrete_powder")]
        public string BlockLimeConcretePowder { get; set; }

        [JsonProperty("block/pink_concrete_powder")]
        public string BlockPinkConcretePowder { get; set; }

        [JsonProperty("block/gray_concrete_powder")]
        public string BlockGrayConcretePowder { get; set; }

        [JsonProperty("block/light_gray_concrete_powder")]
        public string BlockLightGrayConcretePowder { get; set; }

        [JsonProperty("block/cyan_concrete_powder")]
        public string BlockCyanConcretePowder { get; set; }

        [JsonProperty("block/purple_concrete_powder")]
        public string BlockPurpleConcretePowder { get; set; }

        [JsonProperty("block/blue_concrete_powder")]
        public string BlockBlueConcretePowder { get; set; }

        [JsonProperty("block/brown_concrete_powder")]
        public string BlockBrownConcretePowder { get; set; }

        [JsonProperty("block/green_concrete_powder")]
        public string BlockGreenConcretePowder { get; set; }

        [JsonProperty("block/red_concrete_powder")]
        public string BlockRedConcretePowder { get; set; }

        [JsonProperty("block/black_concrete_powder")]
        public string BlockBlackConcretePowder { get; set; }

        [JsonProperty("block/carved_pumpkin")]
        public string BlockCarvedPumpkin { get; set; }

        [JsonProperty("block/jack_o_lantern")]
        public string BlockJackOLantern { get; set; }

        [JsonProperty("block/tripwire")]
        public string BlockTripwire { get; set; }

        [JsonProperty("block/spawner")]
        public string BlockSpawner { get; set; }

        [JsonProperty("block/nether_quartz_ore")]
        public string BlockNetherQuartzOre { get; set; }

        [JsonProperty("block/terracotta")]
        public string BlockTerracotta { get; set; }

        [JsonProperty("block/nether_bricks")]
        public string BlockNetherBricks { get; set; }

        [JsonProperty("block/bricks")]
        public string BlockBricks { get; set; }

        [JsonProperty("block/farmland_moist")]
        public string BlockFarmlandMoist { get; set; }

        [JsonProperty("block/redstone_lamp")]
        public string BlockRedstoneLamp { get; set; }

        [JsonProperty("block/granite")]
        public string BlockGranite { get; set; }

        [JsonProperty("block/fire_0")]
        public string BlockFire0 { get; set; }

        [JsonProperty("block/fire_1")]
        public string BlockFire1 { get; set; }
    }

    public class LegacyItemTextureName
    {
        [JsonProperty("item/golden_helmet")]
        public string ItemGoldenHelmet { get; set; }

        [JsonProperty("item/wheat_seeds")]
        public string ItemWheatSeeds { get; set; }

        [JsonProperty("item/golden_apple")]
        public string ItemGoldenApple { get; set; }

        [JsonProperty("item/golden_chestplate")]
        public string ItemGoldenChestplate { get; set; }

        [JsonProperty("item/bow")]
        public string ItemBow { get; set; }

        [JsonProperty("item/sugar_cane")]
        public string ItemSugarCane { get; set; }

        [JsonProperty("item/slime_ball")]
        public string ItemSlimeBall { get; set; }

        [JsonProperty("item/golden_leggings")]
        public string ItemGoldenLeggings { get; set; }

        [JsonProperty("item/oak_door")]
        public string ItemOakDoor { get; set; }

        [JsonProperty("item/iron_door")]
        public string ItemIronDoor { get; set; }

        [JsonProperty("item/fire_charge")]
        public string ItemFireCharge { get; set; }

        [JsonProperty("item/golden_boots")]
        public string ItemGoldenBoots { get; set; }

        [JsonProperty("item/redstone")]
        public string ItemRedstone { get; set; }

        [JsonProperty("item/book")]
        public string ItemBook { get; set; }

        [JsonProperty("item/pumpkin_seeds")]
        public string ItemPumpkinSeeds { get; set; }

        [JsonProperty("item/melon_seeds")]
        public string ItemMelonSeeds { get; set; }

        [JsonProperty("item/wooden_sword")]
        public string ItemWoodenSword { get; set; }

        [JsonProperty("item/golden_sword")]
        public string ItemGoldenSword { get; set; }

        [JsonProperty("item/fishing_rod")]
        public string ItemFishingRod { get; set; }

        [JsonProperty("item/bucket")]
        public string ItemBucket { get; set; }

        [JsonProperty("item/water_bucket")]
        public string ItemWaterBucket { get; set; }

        [JsonProperty("item/lava_bucket")]
        public string ItemLavaBucket { get; set; }

        [JsonProperty("item/milk_bucket")]
        public string ItemMilkBucket { get; set; }

        [JsonProperty("item/ink_sac")]
        public string ItemInkSac { get; set; }

        [JsonProperty("item/gray_dye")]
        public string ItemGrayDye { get; set; }

        [JsonProperty("item/wooden_shovel")]
        public string ItemWoodenShovel { get; set; }

        [JsonProperty("item/golden_shovel")]
        public string ItemGoldenShovel { get; set; }

        [JsonProperty("item/porkchop")]
        public string ItemPorkchop { get; set; }

        [JsonProperty("item/cooked_porkchop")]
        public string ItemCookedPorkchop { get; set; }

        [JsonProperty("item/cod")]
        public string ItemCod { get; set; }

        [JsonProperty("item/cooked_cod")]
        public string ItemCookedCod { get; set; }

        [JsonProperty("item/rose_red")]
        public string ItemRoseRed { get; set; }

        [JsonProperty("item/pink_dye")]
        public string ItemPinkDye { get; set; }

        [JsonProperty("item/wooden_pickaxe")]
        public string ItemWoodenPickaxe { get; set; }

        [JsonProperty("item/golden_pickaxe")]
        public string ItemGoldenPickaxe { get; set; }

        [JsonProperty("item/beef")]
        public string ItemBeef { get; set; }

        [JsonProperty("item/cooked_beef")]
        public string ItemCookedBeef { get; set; }

        [JsonProperty("item/cactus_green")]
        public string ItemCactusGreen { get; set; }

        [JsonProperty("item/lime_dye")]
        public string ItemLimeDye { get; set; }

        [JsonProperty("item/wooden_axe")]
        public string ItemWoodenAxe { get; set; }

        [JsonProperty("item/golden_axe")]
        public string ItemGoldenAxe { get; set; }

        [JsonProperty("item/baked_potato")]
        public string ItemBakedPotato { get; set; }

        [JsonProperty("item/chicken")]
        public string ItemChicken { get; set; }

        [JsonProperty("item/cooked_chicken")]
        public string ItemCookedChicken { get; set; }

        [JsonProperty("item/cocoa_beans")]
        public string ItemCocoaBeans { get; set; }

        [JsonProperty("item/dandelion_yellow")]
        public string ItemDandelionYellow { get; set; }

        [JsonProperty("item/wooden_hoe")]
        public string ItemWoodenHoe { get; set; }

        [JsonProperty("item/golden_hoe")]
        public string ItemGoldenHoe { get; set; }

        [JsonProperty("item/poisonous_potato")]
        public string ItemPoisonousPotato { get; set; }

        [JsonProperty("item/minecart")]
        public string ItemMinecart { get; set; }

        [JsonProperty("item/speckled_melon")]
        public string ItemSpeckledMelon { get; set; }

        [JsonProperty("item/fermented_spider_eye")]
        public string ItemFermentedSpiderEye { get; set; }

        [JsonProperty("item/potion")]
        public string ItemPotion { get; set; }

        [JsonProperty("item/lapis_lazuli")]
        public string ItemLapisLazuli { get; set; }

        [JsonProperty("item/light_blue_dye")]
        public string ItemLightBlueDye { get; set; }

        [JsonProperty("item/rabbit")]
        public string ItemRabbit { get; set; }

        [JsonProperty("item/cooked_rabbit")]
        public string ItemCookedRabbit { get; set; }

        [JsonProperty("item/golden_carrot")]
        public string ItemGoldenCarrot { get; set; }

        [JsonProperty("item/chest_minecart")]
        public string ItemChestMinecart { get; set; }

        [JsonProperty("item/splash_potion")]
        public string ItemSplashPotion { get; set; }

        [JsonProperty("item/purple_dye")]
        public string ItemPurpleDye { get; set; }

        [JsonProperty("item/magenta_dye")]
        public string ItemMagentaDye { get; set; }

        [JsonProperty("item/mutton")]
        public string ItemMutton { get; set; }

        [JsonProperty("item/cooked_mutton")]
        public string ItemCookedMutton { get; set; }

        [JsonProperty("item/armor_stand")]
        public string ItemArmorStand { get; set; }

        [JsonProperty("item/totem_of_undying")]
        public string ItemTotemOfUndying { get; set; }

        [JsonProperty("item/tnt_minecart")]
        public string ItemTntMinecart { get; set; }

        [JsonProperty("item/furnace_minecart")]
        public string ItemFurnaceMinecart { get; set; }

        [JsonProperty("item/hopper_minecart")]
        public string ItemHopperMinecart { get; set; }

        [JsonProperty("item/cyan_dye")]
        public string ItemCyanDye { get; set; }

        [JsonProperty("item/orange_dye")]
        public string ItemOrangeDye { get; set; }

        [JsonProperty("item/spruce_door")]
        public string ItemSpruceDoor { get; set; }

        [JsonProperty("item/birch_door")]
        public string ItemBirchDoor { get; set; }

        [JsonProperty("item/jungle_door")]
        public string ItemJungleDoor { get; set; }

        [JsonProperty("item/acacia_door")]
        public string ItemAcaciaDoor { get; set; }

        [JsonProperty("item/dark_oak_door")]
        public string ItemDarkOakDoor { get; set; }

        [JsonProperty("item/writable_book")]
        public string ItemWritableBook { get; set; }

        [JsonProperty("item/written")]
        public string ItemWritten { get; set; }

        [JsonProperty("item/light_gray_dye")]
        public string ItemLightGrayDye { get; set; }

        [JsonProperty("item/bone_meal")]
        public string ItemBoneMeal { get; set; }

        [JsonProperty("item/firework_rocket")]
        public string ItemFireworkRocket { get; set; }

        [JsonProperty("item/firework_star")]
        public string ItemFireworkStar { get; set; }

        [JsonProperty("item/firework_star_overlay")]
        public string ItemFireworkStarOverlay { get; set; }

        [JsonProperty("item/map")]
        public string ItemMap { get; set; }

        [JsonProperty("item/map_filled")]
        public string ItemMapFilled { get; set; }

        [JsonProperty("item/map_filled_markings")]
        public string ItemMapFilledMarkings { get; set; }

        [JsonProperty("item/enchanted_book")]
        public string ItemEnchantedBook { get; set; }

        [JsonProperty("item/pufferfish")]
        public string ItemPufferfish { get; set; }

        [JsonProperty("item/salmon")]
        public string ItemSalmon { get; set; }

        [JsonProperty("item/cooked_salmon")]
        public string ItemCookedSalmon { get; set; }

        [JsonProperty("item/glass_bottle")]
        public string ItemGlassBottle { get; set; }

        [JsonProperty("item/command_block_minecart")]
        public string ItemCommandBlockMinecart { get; set; }

        [JsonProperty("item/nether_brick")]
        public string ItemNetherBrick { get; set; }

        [JsonProperty("item/lingering_potion")]
        public string ItemLingeringPotion { get; set; }

        [JsonProperty("item/golden_horse_armor")]
        public string ItemGoldenHorseArmor { get; set; }

        [JsonProperty("item/music_disc_cat")]
        public string ItemMusicDiscCat { get; set; }

        [JsonProperty("item/music_disc_blocks")]
        public string ItemMusicDiscBlocks { get; set; }

        [JsonProperty("item/music_disc_chirp")]
        public string ItemMusicDiscChirp { get; set; }

        [JsonProperty("item/music_disc_far")]
        public string ItemMusicDiscFar { get; set; }

        [JsonProperty("item/music_disc_mall")]
        public string ItemMusicDiscMall { get; set; }

        [JsonProperty("item/music_disc_mellohi")]
        public string ItemMusicDiscMellohi { get; set; }

        [JsonProperty("item/music_disc_stal")]
        public string ItemMusicDiscStal { get; set; }

        [JsonProperty("item/music_disc_strad")]
        public string ItemMusicDiscStrad { get; set; }

        [JsonProperty("item/music_disc_ward")]
        public string ItemMusicDiscWard { get; set; }

        [JsonProperty("item/music_disc_11")]
        public string ItemMusicDisc11 { get; set; }

        [JsonProperty("item/music_disc_wait")]
        public string ItemMusicDiscWait { get; set; }

        [JsonProperty("item/music_disc_13")]
        public string ItemMusicDisc13 { get; set; }

        [JsonProperty("item/seagrass")]
        public string ItemSeagrass { get; set; }

        [JsonProperty("item/melon_slice")]
        public string ItemMelonSlice { get; set; }

        [JsonProperty("item/glistering_melon_slice")]
        public string ItemGlisteringMelonSlice { get; set; }

        [JsonProperty("item/tropical_fish")]
        public string ItemTropicalFish { get; set; }

        [JsonProperty("item/tropical_fish_bucket")]
        public string ItemTropicalFishBucket { get; set; }

        [JsonProperty("item/popped_chorus_fruit")]
        public string ItemPoppedChorusFruit { get; set; }

        [JsonProperty("item/green_dye")]
        public string ItemGreenDye { get; set; }

        [JsonProperty("item/red_dye")]
        public string ItemRedDye { get; set; }

        [JsonProperty("item/yellow_dye")]
        public string ItemYellowDye { get; set; }
    }

    public class LegacyModelId
    {
        [JsonProperty("0")]
        public string _0 { get; set; }

        [JsonProperty("1")]
        public string _1 { get; set; }

        [JsonProperty("2")]
        public string _2 { get; set; }

        [JsonProperty("3")]
        public string _3 { get; set; }

        [JsonProperty("4")]
        public string _4 { get; set; }

        [JsonProperty("5")]
        public string _5 { get; set; }

        [JsonProperty("6")]
        public string _6 { get; set; }

        [JsonProperty("7")]
        public string _7 { get; set; }

        [JsonProperty("8")]
        public string _8 { get; set; }

        [JsonProperty("9")]
        public string _9 { get; set; }

        [JsonProperty("10")]
        public string _10 { get; set; }

        [JsonProperty("11")]
        public string _11 { get; set; }

        [JsonProperty("12")]
        public string _12 { get; set; }

        [JsonProperty("13")]
        public string _13 { get; set; }

        [JsonProperty("14")]
        public string _14 { get; set; }

        [JsonProperty("15")]
        public string _15 { get; set; }

        [JsonProperty("16")]
        public string _16 { get; set; }

        [JsonProperty("17")]
        public string _17 { get; set; }

        [JsonProperty("18")]
        public string _18 { get; set; }

        [JsonProperty("19")]
        public string _19 { get; set; }
    }

    public class LegacyModelId05
    {
        [JsonProperty("20")]
        public string _20 { get; set; }

        [JsonProperty("21")]
        public string _21 { get; set; }
    }

    public class LegacyModelId06
    {
        [JsonProperty("20")]
        public string _20 { get; set; }

        [JsonProperty("21")]
        public string _21 { get; set; }

        [JsonProperty("22")]
        public string _22 { get; set; }

        [JsonProperty("23")]
        public string _23 { get; set; }

        [JsonProperty("24")]
        public string _24 { get; set; }

        [JsonProperty("25")]
        public string _25 { get; set; }

        [JsonProperty("26")]
        public string _26 { get; set; }

        [JsonProperty("27")]
        public string _27 { get; set; }

        [JsonProperty("28")]
        public string _28 { get; set; }

        [JsonProperty("29")]
        public string _29 { get; set; }

        [JsonProperty("30")]
        public string _30 { get; set; }

        [JsonProperty("31")]
        public string _31 { get; set; }

        [JsonProperty("32")]
        public string _32 { get; set; }

        [JsonProperty("33")]
        public string _33 { get; set; }

        [JsonProperty("34")]
        public string _34 { get; set; }

        [JsonProperty("35")]
        public string _35 { get; set; }

        [JsonProperty("36")]
        public string _36 { get; set; }
    }

    public class LegacyModelId100Demo
    {
        [JsonProperty("20")]
        public string _20 { get; set; }

        [JsonProperty("21")]
        public string _21 { get; set; }

        [JsonProperty("22")]
        public string _22 { get; set; }

        [JsonProperty("23")]
        public string _23 { get; set; }

        [JsonProperty("24")]
        public string _24 { get; set; }

        [JsonProperty("25")]
        public string _25 { get; set; }

        [JsonProperty("26")]
        public string _26 { get; set; }

        [JsonProperty("27")]
        public string _27 { get; set; }

        [JsonProperty("28")]
        public string _28 { get; set; }

        [JsonProperty("29")]
        public string _29 { get; set; }

        [JsonProperty("30")]
        public string _30 { get; set; }

        [JsonProperty("31")]
        public string _31 { get; set; }

        [JsonProperty("32")]
        public string _32 { get; set; }

        [JsonProperty("33")]
        public string _33 { get; set; }

        [JsonProperty("34")]
        public string _34 { get; set; }

        [JsonProperty("35")]
        public string _35 { get; set; }

        [JsonProperty("36")]
        public string _36 { get; set; }

        [JsonProperty("37")]
        public string _37 { get; set; }

        [JsonProperty("38")]
        public string _38 { get; set; }

        [JsonProperty("39")]
        public string _39 { get; set; }

        [JsonProperty("40")]
        public string _40 { get; set; }

        [JsonProperty("41")]
        public string _41 { get; set; }

        [JsonProperty("42")]
        public string _42 { get; set; }

        [JsonProperty("43")]
        public string _43 { get; set; }

        [JsonProperty("44")]
        public string _44 { get; set; }

        [JsonProperty("45")]
        public string _45 { get; set; }

        [JsonProperty("46")]
        public string _46 { get; set; }

        [JsonProperty("47")]
        public string _47 { get; set; }
    }

    public class LegacyModelName
    {
        [JsonProperty("characterhuman")]
        public Characterhuman Characterhuman { get; set; }

        [JsonProperty("characterzombie")]
        public Characterzombie Characterzombie { get; set; }

        [JsonProperty("characterskeleton")]
        public Characterskeleton Characterskeleton { get; set; }

        [JsonProperty("charactercreeper")]
        public Charactercreeper Charactercreeper { get; set; }

        [JsonProperty("characterspider")]
        public Characterspider Characterspider { get; set; }

        [JsonProperty("characterenderman")]
        public Characterenderman Characterenderman { get; set; }

        [JsonProperty("characterslime")]
        public Characterslime Characterslime { get; set; }

        [JsonProperty("characterghast")]
        public Characterghast Characterghast { get; set; }

        [JsonProperty("characterzombiepigman")]
        public Characterzombiepigman Characterzombiepigman { get; set; }

        [JsonProperty("characterchicken")]
        public Characterchicken Characterchicken { get; set; }

        [JsonProperty("charactercow")]
        public Charactercow Charactercow { get; set; }

        [JsonProperty("charactermooshroom")]
        public Charactermooshroom Charactermooshroom { get; set; }

        [JsonProperty("characterpig")]
        public Characterpig Characterpig { get; set; }

        [JsonProperty("charactersheep")]
        public Charactersheep Charactersheep { get; set; }

        [JsonProperty("charactersquid")]
        public Charactersquid Charactersquid { get; set; }

        [JsonProperty("charactervillager")]
        public Charactervillager Charactervillager { get; set; }

        [JsonProperty("characterocelot")]
        public Characterocelot Characterocelot { get; set; }

        [JsonProperty("characterwolf")]
        public Characterwolf Characterwolf { get; set; }

        [JsonProperty("characterirongolem")]
        public Characterirongolem Characterirongolem { get; set; }

        [JsonProperty("charactersnowman")]
        public Charactersnowman Charactersnowman { get; set; }

        [JsonProperty("charactersilverfish")]
        public Charactersilverfish Charactersilverfish { get; set; }

        [JsonProperty("characterbat")]
        public Characterbat Characterbat { get; set; }

        [JsonProperty("characterzombievillager")]
        public Characterzombievillager Characterzombievillager { get; set; }

        [JsonProperty("characterwitch")]
        public Characterwitch Characterwitch { get; set; }

        [JsonProperty("charactercavespider")]
        public Charactercavespider Charactercavespider { get; set; }

        [JsonProperty("characterwitherskeleton")]
        public Characterwitherskeleton Characterwitherskeleton { get; set; }

        [JsonProperty("characterwither")]
        public Characterwither Characterwither { get; set; }

        [JsonProperty("characterblaze")]
        public Characterblaze Characterblaze { get; set; }

        [JsonProperty("charactermagmacube")]
        public Charactermagmacube Charactermagmacube { get; set; }

        [JsonProperty("characterhorse")]
        public Characterhorse Characterhorse { get; set; }

        [JsonProperty("characterdonkey")]
        public Characterdonkey Characterdonkey { get; set; }

        [JsonProperty("characterenderdragon")]
        public Characterenderdragon Characterenderdragon { get; set; }

        [JsonProperty("characterendermite")]
        public Characterendermite Characterendermite { get; set; }

        [JsonProperty("characterguardian")]
        public Characterguardian Characterguardian { get; set; }

        [JsonProperty("characterelderguardian")]
        public Characterelderguardian Characterelderguardian { get; set; }

        [JsonProperty("characterrabbit")]
        public Characterrabbit Characterrabbit { get; set; }

        [JsonProperty("charactershulker")]
        public Charactershulker Charactershulker { get; set; }

        [JsonProperty("characteralex")]
        public Characteralex Characteralex { get; set; }

        [JsonProperty("specialblockchest")]
        public Specialblockchest Specialblockchest { get; set; }

        [JsonProperty("specialblocklargechest")]
        public Specialblocklargechest Specialblocklargechest { get; set; }

        [JsonProperty("specialblocklever")]
        public Specialblocklever Specialblocklever { get; set; }

        [JsonProperty("specialblockpiston")]
        public Specialblockpiston Specialblockpiston { get; set; }

        [JsonProperty("specialblockstickypiston")]
        public Specialblockstickypiston Specialblockstickypiston { get; set; }

        [JsonProperty("specialblockarrow")]
        public Specialblockarrow Specialblockarrow { get; set; }

        [JsonProperty("specialblockboat")]
        public Specialblockboat Specialblockboat { get; set; }

        [JsonProperty("specialblockminecart")]
        public Specialblockminecart Specialblockminecart { get; set; }

        [JsonProperty("specialblockenchantmenttable")]
        public Specialblockenchantmenttable Specialblockenchantmenttable { get; set; }

        [JsonProperty("specialblocksignpost")]
        public Specialblocksignpost Specialblocksignpost { get; set; }

        [JsonProperty("specialblockwallsign")]
        public Specialblockwallsign Specialblockwallsign { get; set; }

        [JsonProperty("specialblocktripwirehook")]
        public Specialblocktripwirehook Specialblocktripwirehook { get; set; }

        [JsonProperty("specialblockwoodendoor")]
        public Specialblockwoodendoor Specialblockwoodendoor { get; set; }

        [JsonProperty("specialblockirondoor")]
        public Specialblockirondoor Specialblockirondoor { get; set; }

        [JsonProperty("specialblocktrapdoor")]
        public Specialblocktrapdoor Specialblocktrapdoor { get; set; }

        [JsonProperty("specialblockendercrystal")]
        public Specialblockendercrystal Specialblockendercrystal { get; set; }

        [JsonProperty("specialblockcamera")]
        public Specialblockcamera Specialblockcamera { get; set; }

        [JsonProperty("specialblockbanner")]
        public Specialblockbanner Specialblockbanner { get; set; }

        [JsonProperty("specialblockarmorstand")]
        public Specialblockarmorstand Specialblockarmorstand { get; set; }

        [JsonProperty("specialblockshulkerspark")]
        public Specialblockshulkerspark Specialblockshulkerspark { get; set; }

        [JsonProperty("specialblockshield")]
        public Specialblockshield Specialblockshield { get; set; }

        [JsonProperty("specialblockelytra")]
        public Specialblockelytra Specialblockelytra { get; set; }
    }

    public class LegacyModelNames
    {
        [JsonProperty("fangs")]
        public string Fangs { get; set; }

        [JsonProperty("ender_crystal")]
        public string EnderCrystal { get; set; }

        [JsonProperty("zombie_pigman")]
        public string ZombiePigman { get; set; }

        [JsonProperty("enchantment_table")]
        public string EnchantmentTable { get; set; }

        [JsonProperty("dragon_head")]
        public string DragonHead { get; set; }
    }

    public class LegacyModelPart
    {
        [JsonProperty("human")]
        public List<string> Human { get; set; }

        [JsonProperty("zombie")]
        public List<string> Zombie { get; set; }

        [JsonProperty("skeleton")]
        public List<string> Skeleton { get; set; }

        [JsonProperty("creeper")]
        public List<string> Creeper { get; set; }

        [JsonProperty("spider")]
        public List<string> Spider { get; set; }

        [JsonProperty("enderman")]
        public List<string> Enderman { get; set; }

        [JsonProperty("slime")]
        public List<string> Slime { get; set; }

        [JsonProperty("ghast")]
        public List<string> Ghast { get; set; }

        [JsonProperty("zombie_pigman")]
        public List<string> ZombiePigman { get; set; }

        [JsonProperty("chicken")]
        public List<string> Chicken { get; set; }

        [JsonProperty("cow")]
        public List<string> Cow { get; set; }

        [JsonProperty("pig")]
        public List<string> Pig { get; set; }

        [JsonProperty("sheep")]
        public List<string> Sheep { get; set; }

        [JsonProperty("squid")]
        public List<string> Squid { get; set; }

        [JsonProperty("villager")]
        public List<string> Villager { get; set; }

        [JsonProperty("cat")]
        public List<string> Cat { get; set; }

        [JsonProperty("wolf")]
        public List<string> Wolf { get; set; }

        [JsonProperty("iron_golem")]
        public List<string> IronGolem { get; set; }

        [JsonProperty("snow_golem")]
        public List<string> SnowGolem { get; set; }

        [JsonProperty("silverfish")]
        public List<string> Silverfish { get; set; }

        [JsonProperty("bat")]
        public List<string> Bat { get; set; }

        [JsonProperty("zombie_villager")]
        public List<string> ZombieVillager { get; set; }

        [JsonProperty("witch")]
        public List<string> Witch { get; set; }

        [JsonProperty("wither")]
        public List<string> Wither { get; set; }

        [JsonProperty("blaze")]
        public List<string> Blaze { get; set; }

        [JsonProperty("magma_cube")]
        public List<string> MagmaCube { get; set; }

        [JsonProperty("horse")]
        public List<string> Horse { get; set; }

        [JsonProperty("ender_dragon")]
        public List<string> EnderDragon { get; set; }

        [JsonProperty("endermite")]
        public List<string> Endermite { get; set; }

        [JsonProperty("guardian")]
        public List<string> Guardian { get; set; }

        [JsonProperty("rabbit")]
        public List<string> Rabbit { get; set; }

        [JsonProperty("shulker")]
        public List<string> Shulker { get; set; }

        [JsonProperty("chest")]
        public List<string> Chest { get; set; }

        [JsonProperty("lever")]
        public List<string> Lever { get; set; }

        [JsonProperty("piston")]
        public List<string> Piston { get; set; }

        [JsonProperty("arrow")]
        public List<string> Arrow { get; set; }

        [JsonProperty("boat")]
        public List<string> Boat { get; set; }

        [JsonProperty("minecart")]
        public List<string> Minecart { get; set; }

        [JsonProperty("enchantment_table")]
        public List<string> EnchantmentTable { get; set; }

        [JsonProperty("sign")]
        public List<string> Sign { get; set; }

        [JsonProperty("tripwire_hook")]
        public List<string> TripwireHook { get; set; }

        [JsonProperty("ender_crystal")]
        public List<string> EnderCrystal { get; set; }

        [JsonProperty("camera")]
        public List<string> Camera { get; set; }

        [JsonProperty("banner")]
        public List<string> Banner { get; set; }

        [JsonProperty("armor_stand")]
        public List<string> ArmorStand { get; set; }

        [JsonProperty("shulker_spark")]
        public List<string> ShulkerSpark { get; set; }

        [JsonProperty("shield")]
        public List<string> Shield { get; set; }

        [JsonProperty("elytra")]
        public List<string> Elytra { get; set; }
    }

    public class LegacyModelStates
    {
        [JsonProperty("villager")]
        public Villager Villager { get; set; }

        [JsonProperty("zombie_villager")]
        public ZombieVillager ZombieVillager { get; set; }

        [JsonProperty("snow_golem")]
        public SnowGolem SnowGolem { get; set; }

        [JsonProperty("vex")]
        public Vex Vex { get; set; }

        [JsonProperty("wither")]
        public Wither Wither { get; set; }

        [JsonProperty("strider")]
        public Strider Strider { get; set; }

        [JsonProperty("mob_head")]
        public MobHead MobHead { get; set; }
    }

    public class LegacyModelStateValues
    {
        [JsonProperty("villager")]
        public Villager Villager { get; set; }

        [JsonProperty("zombie_villager")]
        public ZombieVillager ZombieVillager { get; set; }

        [JsonProperty("llama")]
        public Llama Llama { get; set; }

        [JsonProperty("snow_golem")]
        public SnowGolem SnowGolem { get; set; }

        [JsonProperty("vex")]
        public Vex Vex { get; set; }

        [JsonProperty("wither")]
        public Wither Wither { get; set; }

        [JsonProperty("strider")]
        public Strider Strider { get; set; }

        [JsonProperty("shulker")]
        public Shulker Shulker { get; set; }

        [JsonProperty("panda")]
        public Panda Panda { get; set; }

        [JsonProperty("shulker_box")]
        public ShulkerBox ShulkerBox { get; set; }

        [JsonProperty("piglin")]
        public Piglin Piglin { get; set; }

        [JsonProperty("human")]
        public Human Human { get; set; }

        [JsonProperty("wolf")]
        public Wolf Wolf { get; set; }

        [JsonProperty("skeleton")]
        public Skeleton Skeleton { get; set; }
    }

    public class LegacyParticles
    {
        [JsonProperty("particle/generic_0")]
        public List<int> ParticleGeneric0 { get; set; }

        [JsonProperty("particle/generic_1")]
        public List<int> ParticleGeneric1 { get; set; }

        [JsonProperty("particle/generic_2")]
        public List<int> ParticleGeneric2 { get; set; }

        [JsonProperty("particle/generic_3")]
        public List<int> ParticleGeneric3 { get; set; }

        [JsonProperty("particle/generic_4")]
        public List<int> ParticleGeneric4 { get; set; }

        [JsonProperty("particle/generic_5")]
        public List<int> ParticleGeneric5 { get; set; }

        [JsonProperty("particle/generic_6")]
        public List<int> ParticleGeneric6 { get; set; }

        [JsonProperty("particle/generic_7")]
        public List<int> ParticleGeneric7 { get; set; }

        [JsonProperty("particle/splash_0")]
        public List<int> ParticleSplash0 { get; set; }

        [JsonProperty("particle/splash_1")]
        public List<int> ParticleSplash1 { get; set; }

        [JsonProperty("particle/splash_2")]
        public List<int> ParticleSplash2 { get; set; }

        [JsonProperty("particle/splash_3")]
        public List<int> ParticleSplash3 { get; set; }

        [JsonProperty("particle/bubble")]
        public List<int> ParticleBubble { get; set; }

        [JsonProperty("particle/flame")]
        public List<int> ParticleFlame { get; set; }

        [JsonProperty("particle/lava")]
        public List<int> ParticleLava { get; set; }

        [JsonProperty("particle/note")]
        public List<int> ParticleNote { get; set; }

        [JsonProperty("particle/critical_hit")]
        public List<int> ParticleCriticalHit { get; set; }

        [JsonProperty("particle/enchanted_hit")]
        public List<int> ParticleEnchantedHit { get; set; }

        [JsonProperty("particle/damage")]
        public List<int> ParticleDamage { get; set; }

        [JsonProperty("particle/heart")]
        public List<int> ParticleHeart { get; set; }

        [JsonProperty("particle/angry")]
        public List<int> ParticleAngry { get; set; }

        [JsonProperty("particle/glint")]
        public List<int> ParticleGlint { get; set; }

        [JsonProperty("particle/drip_fall")]
        public List<int> ParticleDripFall { get; set; }

        [JsonProperty("particle/drip_hang")]
        public List<int> ParticleDripHang { get; set; }

        [JsonProperty("particle/drip_land")]
        public List<int> ParticleDripLand { get; set; }

        [JsonProperty("particle/effect_0")]
        public List<int> ParticleEffect0 { get; set; }

        [JsonProperty("particle/effect_1")]
        public List<int> ParticleEffect1 { get; set; }

        [JsonProperty("particle/effect_2")]
        public List<int> ParticleEffect2 { get; set; }

        [JsonProperty("particle/effect_3")]
        public List<int> ParticleEffect3 { get; set; }

        [JsonProperty("particle/effect_4")]
        public List<int> ParticleEffect4 { get; set; }

        [JsonProperty("particle/effect_5")]
        public List<int> ParticleEffect5 { get; set; }

        [JsonProperty("particle/effect_6")]
        public List<int> ParticleEffect6 { get; set; }

        [JsonProperty("particle/effect_7")]
        public List<int> ParticleEffect7 { get; set; }

        [JsonProperty("particle/spell_0")]
        public List<int> ParticleSpell0 { get; set; }

        [JsonProperty("particle/spell_1")]
        public List<int> ParticleSpell1 { get; set; }

        [JsonProperty("particle/spell_2")]
        public List<int> ParticleSpell2 { get; set; }

        [JsonProperty("particle/spell_3")]
        public List<int> ParticleSpell3 { get; set; }

        [JsonProperty("particle/spell_4")]
        public List<int> ParticleSpell4 { get; set; }

        [JsonProperty("particle/spell_5")]
        public List<int> ParticleSpell5 { get; set; }

        [JsonProperty("particle/spell_6")]
        public List<int> ParticleSpell6 { get; set; }

        [JsonProperty("particle/spell_7")]
        public List<int> ParticleSpell7 { get; set; }

        [JsonProperty("particle/spark_0")]
        public List<int> ParticleSpark0 { get; set; }

        [JsonProperty("particle/spark_1")]
        public List<int> ParticleSpark1 { get; set; }

        [JsonProperty("particle/spark_2")]
        public List<int> ParticleSpark2 { get; set; }

        [JsonProperty("particle/spark_3")]
        public List<int> ParticleSpark3 { get; set; }

        [JsonProperty("particle/spark_4")]
        public List<int> ParticleSpark4 { get; set; }

        [JsonProperty("particle/spark_5")]
        public List<int> ParticleSpark5 { get; set; }

        [JsonProperty("particle/spark_6")]
        public List<int> ParticleSpark6 { get; set; }

        [JsonProperty("particle/spark_7")]
        public List<int> ParticleSpark7 { get; set; }

        [JsonProperty("particle/glitter_0")]
        public List<int> ParticleGlitter0 { get; set; }

        [JsonProperty("particle/glitter_1")]
        public List<int> ParticleGlitter1 { get; set; }

        [JsonProperty("particle/glitter_2")]
        public List<int> ParticleGlitter2 { get; set; }

        [JsonProperty("particle/glitter_3")]
        public List<int> ParticleGlitter3 { get; set; }

        [JsonProperty("particle/glitter_4")]
        public List<int> ParticleGlitter4 { get; set; }

        [JsonProperty("particle/glitter_5")]
        public List<int> ParticleGlitter5 { get; set; }

        [JsonProperty("particle/glitter_6")]
        public List<int> ParticleGlitter6 { get; set; }

        [JsonProperty("particle/glitter_7")]
        public List<int> ParticleGlitter7 { get; set; }

        [JsonProperty("particle/sga_a")]
        public List<int> ParticleSgaA { get; set; }

        [JsonProperty("particle/sga_b")]
        public List<int> ParticleSgaB { get; set; }

        [JsonProperty("particle/sga_c")]
        public List<int> ParticleSgaC { get; set; }

        [JsonProperty("particle/sga_d")]
        public List<int> ParticleSgaD { get; set; }

        [JsonProperty("particle/sga_e")]
        public List<int> ParticleSgaE { get; set; }

        [JsonProperty("particle/sga_f")]
        public List<int> ParticleSgaF { get; set; }

        [JsonProperty("particle/sga_g")]
        public List<int> ParticleSgaG { get; set; }

        [JsonProperty("particle/sga_h")]
        public List<int> ParticleSgaH { get; set; }

        [JsonProperty("particle/sga_i")]
        public List<int> ParticleSgaI { get; set; }

        [JsonProperty("particle/sga_j")]
        public List<int> ParticleSgaJ { get; set; }

        [JsonProperty("particle/sga_k")]
        public List<int> ParticleSgaK { get; set; }

        [JsonProperty("particle/sga_l")]
        public List<int> ParticleSgaL { get; set; }

        [JsonProperty("particle/sga_m")]
        public List<int> ParticleSgaM { get; set; }

        [JsonProperty("particle/sga_n")]
        public List<int> ParticleSgaN { get; set; }

        [JsonProperty("particle/sga_o")]
        public List<int> ParticleSgaO { get; set; }

        [JsonProperty("particle/sga_p")]
        public List<int> ParticleSgaP { get; set; }

        [JsonProperty("particle/sga_q")]
        public List<int> ParticleSgaQ { get; set; }

        [JsonProperty("particle/sga_r")]
        public List<int> ParticleSgaR { get; set; }

        [JsonProperty("particle/sga_s")]
        public List<int> ParticleSgaS { get; set; }

        [JsonProperty("particle/sga_t")]
        public List<int> ParticleSgaT { get; set; }

        [JsonProperty("particle/sga_u")]
        public List<int> ParticleSgaU { get; set; }

        [JsonProperty("particle/sga_v")]
        public List<int> ParticleSgaV { get; set; }

        [JsonProperty("particle/sga_w")]
        public List<int> ParticleSgaW { get; set; }

        [JsonProperty("particle/sga_x")]
        public List<int> ParticleSgaX { get; set; }

        [JsonProperty("particle/sga_y")]
        public List<int> ParticleSgaY { get; set; }

        [JsonProperty("particle/sga_z")]
        public List<int> ParticleSgaZ { get; set; }

        [JsonProperty("particle/explosion_0")]
        public List<int> ParticleExplosion0 { get; set; }

        [JsonProperty("particle/explosion_1")]
        public List<int> ParticleExplosion1 { get; set; }

        [JsonProperty("particle/explosion_2")]
        public List<int> ParticleExplosion2 { get; set; }

        [JsonProperty("particle/explosion_3")]
        public List<int> ParticleExplosion3 { get; set; }

        [JsonProperty("particle/explosion_4")]
        public List<int> ParticleExplosion4 { get; set; }

        [JsonProperty("particle/explosion_5")]
        public List<int> ParticleExplosion5 { get; set; }

        [JsonProperty("particle/explosion_6")]
        public List<int> ParticleExplosion6 { get; set; }

        [JsonProperty("particle/explosion_7")]
        public List<int> ParticleExplosion7 { get; set; }

        [JsonProperty("particle/explosion_8")]
        public List<int> ParticleExplosion8 { get; set; }

        [JsonProperty("particle/explosion_9")]
        public List<int> ParticleExplosion9 { get; set; }

        [JsonProperty("particle/explosion_10")]
        public List<int> ParticleExplosion10 { get; set; }

        [JsonProperty("particle/explosion_11")]
        public List<int> ParticleExplosion11 { get; set; }

        [JsonProperty("particle/explosion_12")]
        public List<int> ParticleExplosion12 { get; set; }

        [JsonProperty("particle/explosion_13")]
        public List<int> ParticleExplosion13 { get; set; }

        [JsonProperty("particle/explosion_14")]
        public List<int> ParticleExplosion14 { get; set; }

        [JsonProperty("particle/explosion_15")]
        public List<int> ParticleExplosion15 { get; set; }
    }

    public class Llama
    {
        [JsonProperty("variant")]
        public Variant Variant { get; set; }
    }

    public class MobHead
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Panda
    {
        [JsonProperty("variant")]
        public Variant Variant { get; set; }
    }

    public class Piglin
    {
        [JsonProperty("variant")]
        public Variant Variant { get; set; }
    }

    public class Pumpkin
    {
        [JsonProperty("snow_golem")]
        public string SnowGolem { get; set; }

        [JsonProperty("snowman")]
        public string Snowman { get; set; }
    }

    public class Root
    {
        [JsonProperty("legacy_model_id")]
        public LegacyModelId LegacyModelId { get; set; }

        [JsonProperty("legacy_model_id_05")]
        public LegacyModelId05 LegacyModelId05 { get; set; }

        [JsonProperty("legacy_model_id_06")]
        public LegacyModelId06 LegacyModelId06 { get; set; }

        [JsonProperty("legacy_model_id_100_demo")]
        public LegacyModelId100Demo LegacyModelId100Demo { get; set; }

        [JsonProperty("legacy_model_name")]
        public LegacyModelName LegacyModelName { get; set; }

        [JsonProperty("legacy_model_part")]
        public LegacyModelPart LegacyModelPart { get; set; }

        [JsonProperty("legacy_block_id")]
        public LegacyBlockId LegacyBlockId { get; set; }

        [JsonProperty("legacy_block_texture_name")]
        public LegacyBlockTextureName LegacyBlockTextureName { get; set; }

        [JsonProperty("legacy_block_05_textures")]
        public List<string> LegacyBlock05Textures { get; set; }

        [JsonProperty("legacy_block_07_demo_textures")]
        public List<string> LegacyBlock07DemoTextures { get; set; }

        [JsonProperty("legacy_block_100_textures")]
        public List<string> LegacyBlock100Textures { get; set; }

        [JsonProperty("legacy_item_texture_name")]
        public LegacyItemTextureName LegacyItemTextureName { get; set; }

        [JsonProperty("legacy_biomes")]
        public LegacyBiomes LegacyBiomes { get; set; }

        [JsonProperty("legacy_model_names")]
        public LegacyModelNames LegacyModelNames { get; set; }

        [JsonProperty("legacy_model_states")]
        public LegacyModelStates LegacyModelStates { get; set; }

        [JsonProperty("legacy_model_state_values")]
        public LegacyModelStateValues LegacyModelStateValues { get; set; }

        [JsonProperty("legacy_block_names")]
        public LegacyBlockNames LegacyBlockNames { get; set; }

        [JsonProperty("legacy_block_states")]
        public LegacyBlockStates LegacyBlockStates { get; set; }

        [JsonProperty("legacy_block_state_values")]
        public LegacyBlockStateValues LegacyBlockStateValues { get; set; }

        [JsonProperty("legacy_particles")]
        public LegacyParticles LegacyParticles { get; set; }

        [JsonProperty("legacy_biome_ids")]
        public LegacyBiomeIds LegacyBiomeIds { get; set; }

        [JsonProperty("biome_ids")]
        public BiomeIds BiomeIds { get; set; }
    }

    public class Shulker
    {
        [JsonProperty("variant")]
        public Variant Variant { get; set; }
    }

    public class ShulkerBox
    {
        [JsonProperty("variant")]
        public Variant Variant { get; set; }
    }

    public class Skeleton
    {
        [JsonProperty("variant")]
        public Variant Variant { get; set; }
    }

    public class SnowGolem
    {
        [JsonProperty("variant")]
        public string Variant { get; set; }

        [JsonProperty("pumpkin")]
        public Pumpkin Pumpkin { get; set; }
    }

    public class Specialblockarmorstand
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Specialblockarrow
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Specialblockbanner
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Specialblockboat
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Specialblockcamera
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Specialblockchest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Specialblockelytra
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Specialblockenchantmenttable
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Specialblockendercrystal
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Specialblockirondoor
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Specialblocklargechest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Specialblocklever
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Specialblockminecart
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Specialblockpiston
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Specialblockshield
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Specialblockshulkerspark
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Specialblocksignpost
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Specialblockstickypiston
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Specialblocktrapdoor
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Specialblocktripwirehook
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Specialblockwallsign
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class Specialblockwoodendoor
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Steve
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Strider
    {
        [JsonProperty("cold")]
        public string Cold { get; set; }

        [JsonProperty("variant")]
        public Variant Variant { get; set; }
    }

    public class Type
    {
        [JsonProperty("grass")]
        public string Grass { get; set; }
    }

    public class Variant
    {
        [JsonProperty("villager")]
        public string Villager { get; set; }

        [JsonProperty("smith")]
        public string Smith { get; set; }

        [JsonProperty("priest")]
        public string Priest { get; set; }

        [JsonProperty("normal")]
        public string Normal { get; set; }

        [JsonProperty("brute")]
        public string Brute { get; set; }

        [JsonProperty("zombified")]
        public string Zombified { get; set; }

        [JsonProperty("alex")]
        public Alex Alex { get; set; }

        [JsonProperty("steve")]
        public Steve Steve { get; set; }

        [JsonProperty("tame")]
        public string Tame { get; set; }

        [JsonProperty("angry")]
        public string Angry { get; set; }

        [JsonProperty("stray_skeleton")]
        public string StraySkeleton { get; set; }

        [JsonProperty("false")]
        public string False { get; set; }

        [JsonProperty("true")]
        public string True { get; set; }

        [JsonProperty("none")]
        public string None { get; set; }
    }

    public class Vex
    {
        [JsonProperty("charging")]
        public string Charging { get; set; }

        [JsonProperty("variant")]
        public Variant Variant { get; set; }
    }

    public class Villager
    {
        [JsonProperty("variant")]
        public string Variant { get; set; }
    }

    public class Wither
    {
        [JsonProperty("invulnerable")]
        public string Invulnerable { get; set; }

        [JsonProperty("variant")]
        public Variant Variant { get; set; }
    }

    public class Wolf
    {
        [JsonProperty("variant")]
        public Variant Variant { get; set; }
    }

    public class ZombieVillager
    {
        [JsonProperty("variant")]
        public string Variant { get; set; }
    }
}