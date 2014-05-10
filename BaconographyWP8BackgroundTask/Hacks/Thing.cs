//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.Serialization;
//using System.Text;
//using System.Threading.Tasks;

//namespace BaconographyWP8BackgroundTask.Hacks
//{
//    public interface IThingData
//    {
//    }

//    [DataContract]
//    public class ThingData : IThingData
//    {
//        [JsonProperty("id")]
//        public string Id { get; set; }
//        [JsonProperty("name")]
//        public string Name { get; set; }
//    }
//    [DataContract]
//    [JsonConverter(typeof(ThingDataConverter))]
//    public class Thing
//    {

//        [JsonProperty("kind")]
//        public string Kind { get; set; }
//        [JsonProperty("data")]
//        public IThingData Data { get; set; }
//    }

//    [DataContract]
//    [JsonConverter(typeof(TypedThingDataConverter))]
//    public class TypedThing<T> : Thing where T : class ,IThingData
//    {
//        public TypedThing(Thing thing)
//        {
//            if (thing == null || !(thing.Data is T))
//                throw new ArgumentException("thing null or incorrect data type");

//            Kind = thing.Kind;
//            base.Data = thing.Data;
//        }
//        [JsonProperty("data")]
//        public new T Data
//        {
//            get
//            {
//                return base.Data as T;
//            }
//            set
//            {
//                base.Data = value;
//            }
//        }

//        // XAML friendly data wrapper
//        public T TypedData
//        {
//            get
//            {
//                return Data;
//            }
//        }

//        //[JsonConstructor()]
//        public TypedThing(string kind, T data)
//        {
//            base.Kind = kind;
//            base.Data = data;
//        }
//    }

//    public class ThingDataConverter : JsonConverter
//    {
//        public override bool CanConvert(Type objectType)
//        {
//            return objectType == typeof(Thing);
//        }

//        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            Thing targetThing = new Thing();

//            if (reader.TokenType == JsonToken.Null)
//                return null;

//            while (reader.TokenType != JsonToken.EndObject)
//            {
//                reader.Read(); // startobject

//                switch ((string)reader.Value)
//                {
//                    case "kind":
//                        {
//                            reader.Read(); //get the kind value
//                            targetThing.Kind = (string)reader.Value;

//                            switch (targetThing.Kind)
//                            {
//                                case "t2":
//                                    targetThing.Data = new Account();
//                                    break;
//                                case "t3":
//                                    targetThing.Data = new Link();
//                                    break;
//                                case "t4":
//                                    targetThing.Data = new Message();
//                                    break;
//                                case "t4.5":
//                                    targetThing.Data = new CommentMessage();
//                                    break;
//                                default:
//                                    throw new NotImplementedException();
//                            }
//                            break;
//                        }
//                    case "data":
//                        {
//                            reader.Read(); //move to inner object
//                            serializer.Populate(reader, targetThing.Data);
//                            break;
//                        }
//                    default:
//                        throw new NotImplementedException();
//                }
//            }
//            reader.Read(); //close the current object
//            return targetThing;
//        }

//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        {
//            writer.WriteStartObject();
//            writer.WritePropertyName("kind");
//            writer.WriteValue(((Thing)value).Kind);
//            writer.WritePropertyName("data");
//            serializer.Serialize(writer, ((Thing)value).Data);

//            writer.WriteEndObject();
//        }
//    }

//    public class TypedThingDataConverter : JsonConverter
//    {
//        public override bool CanConvert(Type objectType)
//        {
//            return objectType == typeof(Thing);
//        }

//        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            Thing targetThing = new Thing();
//            Type dataType = typeof(Thing);

//            if (reader.TokenType == JsonToken.Null)
//                return null;

//            while (reader.TokenType != JsonToken.EndObject)
//            {
//                reader.Read(); // startobject

//                switch ((string)reader.Value)
//                {
//                    case "kind":
//                        {
//                            reader.Read(); //get the kind value
//                            targetThing.Kind = (string)reader.Value;

//                            switch (targetThing.Kind)
//                            {
//                                case "t2":
//                                    targetThing.Data = new Account();
//                                    dataType = typeof(Account);
//                                    break;
//                                case "t3":
//                                    targetThing.Data = new Link();
//                                    dataType = typeof(Link);
//                                    break;
//                                case "t4":
//                                    targetThing.Data = new Message();
//                                    dataType = typeof(Message);
//                                    break;
//                                case "t4.5":
//                                    targetThing.Data = new CommentMessage();
//                                    dataType = typeof(CommentMessage);
//                                    break;
//                                default:
//                                    throw new NotImplementedException();
//                            }
//                            break;
//                        }
//                    case "data":
//                        {
//                            reader.Read(); //move to inner object
//                            serializer.Populate(reader, targetThing.Data);
//                            break;
//                        }
//                    default:
//                        throw new NotImplementedException();
//                }
//            }
//            reader.Read(); //close the current object

//            Type genericType = typeof(TypedThing<>);
//            Type[] typeArgs = { dataType };
//            Type typedThing = genericType.MakeGenericType(typeArgs);

//            return Activator.CreateInstance(typedThing, new object[] { targetThing });
//        }

//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        {
//            writer.WriteStartObject();
//            writer.WritePropertyName("kind");
//            writer.WriteValue(((Thing)value).Kind);
//            writer.WritePropertyName("data");
//            serializer.Serialize(writer, ((Thing)value).Data);

//            writer.WriteEndObject();
//        }
//    }

//    [DataContract]
//    public class CommentMessage : Message
//    {
//        [JsonProperty("link_title")]
//        public string LinkTitle { get; set; }
//        [JsonProperty("likes")]
//        public bool? Likes { get; set; }
//    }

//    [DataContract]
//    public class Link : ThingData
//    {
//        [JsonProperty("created")]
//        public string Created { get; set; }
//        [JsonProperty("created_utc")]

//        public string CreatedUTC { get; set; }
//        [JsonProperty("author")]
//        public string Author { get; set; }
//        [JsonProperty("author_flair_css_class")]
//        public string AuthorFlairCssClass { get; set; }
//        [JsonProperty("author_flair_text")]
//        public string AuthorFlairText { get; set; }
//        [JsonProperty("clicked")]
//        public bool Clicked { get; set; }
//        [JsonProperty("domain")]
//        public string Domain { get; set; }
//        [JsonProperty("hidden")]
//        public bool Hidden { get; set; }
//        [JsonProperty("is_self")]
//        public bool IsSelf { get; set; }
//        [JsonProperty("media")]
//        public object Media { get; set; }
//        [JsonProperty("media_embed")]
//        public MediaEmbed MediaEmbed { get; set; }
//        [JsonProperty("num_comments")]
//        public int CommentCount { get; set; }
//        [JsonProperty("over_18")]
//        public bool Over18 { get; set; }
//        [JsonProperty("permalink")]
//        public string Permalink { get; set; }
//        [JsonProperty("saved")]
//        public bool Saved { get; set; }
//        [JsonProperty("score")]
//        public int Score { get; set; }
//        [JsonProperty("selftext")]
//        public string Selftext { get; set; }
//        [JsonProperty("selftext_html")]
//        public string SelftextHtml { get; set; }
//        [JsonProperty("subreddit")]
//        public string Subreddit { get; set; }
//        [JsonProperty("subreddit_id")]
//        public string SubredditId { get; set; }
//        [JsonProperty("thumbnail")]
//        public string Thumbnail { get; set; }
//        [JsonProperty("title")]
//        public string Title { get; set; }
//        [JsonProperty("url")]
//        public string Url { get; set; }

//        [JsonProperty("ups")]
//        public int Ups { get; set; }
//        [JsonProperty("downs")]
//        public int Downs { get; set; }
//        [JsonProperty("likes")]
//        public bool? Likes { get; set; }
//    }

//    public class MediaEmbed
//    {
//        [JsonProperty("content")]
//        public string Content { get; set; }
//        [JsonProperty("width")]
//        public int Width { get; set; }
//        [JsonProperty("height")]
//        public int Height { get; set; }
//    }

//    [DataContract]
//    public class Message : ThingData
//    {
//        [JsonProperty("author")]
//        public string Author { get; set; }
//        [JsonProperty("body")]
//        public string Body { get; set; }
//        [JsonProperty("body_html")]
//        public string BodyHtml { get; set; }
//        [JsonProperty("context")]
//        public string Context { get; set; }
//        [JsonProperty("created")]
//        public string Created { get; set; }
//        [JsonProperty("created_utc")]
//        public string CreatedUTC { get; set; }
//        [JsonProperty("dest")]
//        public string Destination { get; set; }
//        [JsonProperty("first_message")]
//        public object FirstMessage { get; set; }
//        [JsonProperty("first_message_name")]
//        public string FirstMessageName { get; set; }
//        [JsonProperty("id")]
//        public string Id { get; set; }
//        [JsonProperty("name")]
//        public string Name { get; set; }
//        [JsonProperty("new")]
//        public bool New { get; set; }
//        [JsonProperty("parent_id")]
//        public string ParentId { get; set; }
//        [JsonProperty("replies")]
//        public string Replies { get; set; }
//        [JsonProperty("subject")]
//        public string Subject { get; set; }
//        [JsonProperty("subreddit")]
//        public string Subreddit { get; set; }
//        [JsonProperty("was_comment")]
//        public bool WasComment { get; set; }
//    }
//    [DataContract]
//    public class User
//    {
//        public string Username { get; set; }
//        public string LoginCookie { get; set; }
//        public bool Authenticated { get; set; }
//        public Account Me { get; set; }
//        public bool NeedsCaptcha { get; set; }
//    }

//    public class Account : ThingData
//    {
//        [JsonProperty("comment_karma")]
//        public int CommentKarma { get; set; }
//        [JsonProperty("created")]
//        public string Created { get; set; }
//        [JsonProperty("created_utc")]
//        public string CreatedUTC { get; set; }

//        [JsonProperty("has_mail", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate)]
//        public bool HasMail { get; set; }
//        [JsonProperty("has_mod_mail", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate)]
//        public bool HasModMail { get; set; }
//        [JsonProperty("id")]
//        public string Id { get; set; }
//        [JsonProperty("is_gold", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate)]
//        public bool IsGold { get; set; }
//        [JsonProperty("is_mod", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate)]
//        public bool IsMod { get; set; }
//        [JsonProperty("link_karma", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate)]
//        public int LinkKarma { get; set; }
//        [JsonProperty("modhash")]
//        public string ModHash { get; set; }
//        [JsonProperty("name")]
//        public string Name { get; set; }
//    }

//    [DataContract]
//    public class UserCredential
//    {
//        [JsonProperty("logincookie")]
//        public string LoginCookie { get; set; }
//        [JsonProperty("username")]
//        public string Username { get; set; }
//        [JsonProperty("me")]
//        public Thing Me { get; set; }
//        [JsonProperty("isdefault")]
//        public bool IsDefault { get; set; }
//    }

//    [DataContract]
//    public class Listing
//    {
//        [JsonProperty("kind")]
//        public string Kind { get; set; }
//        [JsonProperty("data")]
//        public ListingData Data { get; set; }
//    }

//    [DataContract]
//    public class ListingData
//    {
//        [JsonProperty("modhash")]
//        public string ModHash { get; set; }

//        [JsonProperty("children")]
//        public List<Thing> Children { get; set; }

//        [JsonProperty("after")]
//        public string After { get; set; }
//        [JsonProperty("before")]
//        public string Before { get; set; }
//    }

//    public class JsonThing
//    {
//        [JsonProperty("json")]
//        public JsonData Json { get; set; }
//    }

//    public class LoginJsonThing
//    {
//        [JsonProperty("json")]
//        public LoginJsonData Json { get; set; }
//    }

//    public class LoginJsonData : IThingData
//    {
//        [JsonProperty("data")]
//        public JsonData3 Data { get; set; }
//        [JsonProperty("errors")]
//        public Object[] Errors { get; set; }
//    }

//    public class JsonData3 : IThingData
//    {
//        [JsonProperty("modhash")]
//        public string Modhash { get; set; }
//        [JsonProperty("cookie")]
//        public string Cookie { get; set; }
//    }

//    public class JsonData : IThingData
//    {
//        [JsonProperty("data")]
//        public JsonData2 Data { get; set; }
//        [JsonProperty("errors")]
//        public Object[] Errors { get; set; }
//    }

//    public class JsonData2 : IThingData
//    {
//        [JsonProperty("things")]
//        public List<Thing> Things { get; set; }
//    }

//    [DataContract]
//    public class CaptchaJsonData : IThingData
//    {
//        [JsonProperty("captcha")]
//        public string Captcha { get; set; }
//        [JsonProperty("errors")]
//        public string[] Errors { get; set; }
//    }
//}


