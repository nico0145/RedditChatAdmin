using System;
using System.Collections.Generic;
using System.Text;

namespace SendBirdAPI
{
    public class Member
    {
        public object friend_discovery_key { get; set; }
        public object friend_name { get; set; }
        public bool is_active { get; set; }
        public bool is_blocked_by_me { get; set; }
        public bool is_blocking_me { get; set; }
        public bool is_online { get; set; }
        public int? joined_ts { get; set; }
        public int last_seen_at { get; set; }
        public Metadata metadata { get; set; }
        public string nickname { get; set; }
        public string profile_url { get; set; }
        public string role { get; set; }
        public string state { get; set; }
        public string user_id { get; set; }
        public override string ToString()
        {
            return $"Nickname: {nickname}" + Environment.NewLine +
                   $"User_Id: {user_id}" + Environment.NewLine;
        }
    }

    public class SearchMemberResult
    {
        public IList<Member> members { get; set; }
        public string next { get; set; }
    }

    public class sbTokenResponse
    {
        public long valid_until { get; set; }
        public string sb_access_token { get; set; }
    }
    public class Metadata
    {
    }

    public class User
    {
        public string phone_number { get; set; }
        public string nickname { get; set; }
        public string user_id { get; set; }
        public string profile_url { get; set; }
        public Metadata metadata { get; set; }
        public override string ToString()
        {
            return $"Nickname: {nickname}" + Environment.NewLine +
                   $"User_Id: {user_id}" + Environment.NewLine;
        }
    }
    public class MutedList
    {
        public string description { get; set; }
        public double end_at { get; set; }
        public Metadata metadata { get; set; }
        public string nickname { get; set; }
        public object phone_number { get; set; }
        public string profile_url { get; set; }
        public int remaining_duration { get; set; }
        public double start_at { get; set; }
        public string user_id { get; set; }
        public override string ToString()
        {
            return $"Nickname: {nickname}" + Environment.NewLine +
                   $"User_Id: {user_id}" + Environment.NewLine +
                   $"Muted Since: {StartAtDate.ToShortDateString()}" + Environment.NewLine +
                   $"Muted Until: {EndAtDate.ToShortDateString()}" + Environment.NewLine;
        }
        public DateTime EndAtDate
        {
            get
            {
                return UnixTimeStampToDateTime(end_at);
            }
        }
        public DateTime StartAtDate
        {
            get
            {
                return UnixTimeStampToDateTime(start_at);
            }
        }
        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp / 1000).ToLocalTime();
            return dtDateTime;
        }
    }

    public class MutedListResult
    {
        public IList<MutedList> muted_list { get; set; }
        public string next { get; set; }
    }
    public class BannedList
    {
        public string description { get; set; }
        public double start_at { get; set; }
        public User user { get; set; }
        public double end_at { get; set; }
        public DateTime EndAtDate
        {
            get
            {
                return UnixTimeStampToDateTime(end_at);
            }
        }
        public DateTime StartAtDate
        {
            get
            {
                return UnixTimeStampToDateTime(start_at);
            }
        }
        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp / 1000).ToLocalTime();
            return dtDateTime;
        }
        public override string ToString()
        {
            return $"Nickname: {user.nickname}" + Environment.NewLine +
                   $"User_Id: {user.user_id}" + Environment.NewLine +
                   $"Banned Since: {StartAtDate.ToShortDateString()}" + Environment.NewLine +
                   $"Banned Until: {EndAtDate.ToShortDateString()}" + Environment.NewLine;
        }
    }

    public class BanList
    {
        public IList<BannedList> banned_list { get; set; }
        public string next { get; set; }
    }
    public class Subreddit
    {
        public bool default_set { get; set; }
        public bool user_is_contributor { get; set; }
        public string banner_img { get; set; }
        public bool restrict_posting { get; set; }
        public bool user_is_banned { get; set; }
        public bool free_form_reports { get; set; }
        public object community_icon { get; set; }
        public bool show_media { get; set; }
        public string icon_color { get; set; }
        public bool user_is_muted { get; set; }
        public string display_name { get; set; }
        public object header_img { get; set; }
        public string title { get; set; }
        public int coins { get; set; }
        public IList<object> previous_names { get; set; }
        public bool over_18 { get; set; }
        public IList<int> icon_size { get; set; }
        public string primary_color { get; set; }
        public string icon_img { get; set; }
        public string description { get; set; }
        public string submit_link_label { get; set; }
        public object header_size { get; set; }
        public bool restrict_commenting { get; set; }
        public int subscribers { get; set; }
        public string submit_text_label { get; set; }
        public bool is_default_icon { get; set; }
        public string link_flair_position { get; set; }
        public string display_name_prefixed { get; set; }
        public string key_color { get; set; }
        public string name { get; set; }
        public bool is_default_banner { get; set; }
        public string url { get; set; }
        public object banner_size { get; set; }
        public bool user_is_moderator { get; set; }
        public string public_description { get; set; }
        public bool link_flair_enabled { get; set; }
        public bool disable_contributor_requests { get; set; }
        public string subreddit_type { get; set; }
        public bool user_is_subscriber { get; set; }
    }

    public class MwebSharingWebShareApi
    {
        public string owner { get; set; }
        public string variant { get; set; }
        public int experiment_id { get; set; }
    }

    public class FeedAdLoad3
    {
        public string owner { get; set; }
        public string variant { get; set; }
        public int experiment_id { get; set; }
    }

    public class Features
    {
        public bool promoted_trend_blanks { get; set; }
        public bool show_amp_link { get; set; }
        public bool chat { get; set; }
        public bool reports_double_write_to_report_service_for_spam { get; set; }
        public bool twitter_embed { get; set; }
        public bool is_email_permission_required { get; set; }
        public bool mod_awards { get; set; }
        public bool expensive_coins_package { get; set; }
        public bool chat_subreddit { get; set; }
        public bool awards_on_streams { get; set; }
        public bool mweb_xpromo_modal_listing_click_daily_dismissible_ios { get; set; }
        public bool modlog_copyright_removal { get; set; }
        public bool show_nps_survey { get; set; }
        public bool do_not_track { get; set; }
        public bool chat_user_settings { get; set; }
        public bool custom_feeds { get; set; }
        public bool mweb_xpromo_interstitial_comments_ios { get; set; }
        public bool mweb_xpromo_modal_listing_click_daily_dismissible_android { get; set; }
        public bool premium_subscriptions_table { get; set; }
        public bool mweb_xpromo_interstitial_comments_android { get; set; }
        public bool stream_as_a_post_type { get; set; }
        public MwebSharingWebShareApi mweb_sharing_web_share_api { get; set; }
        public bool chat_group_rollout { get; set; }
        public bool resized_styles_images { get; set; }
        public bool spez_modal { get; set; }
        public bool noreferrer_to_noopener { get; set; }
        public FeedAdLoad3 feed_ad_load_3 { get; set; }
    }

    public class Data
    {
        public bool is_employee { get; set; }
        public bool has_visited_new_profile { get; set; }
        public bool is_friend { get; set; }
        public bool pref_no_profanity { get; set; }
        public bool has_external_account { get; set; }
        public string pref_geopopular { get; set; }
        public bool pref_show_trending { get; set; }
        public Subreddit subreddit { get; set; }
        public bool is_sponsor { get; set; }
        public object gold_expiration { get; set; }
        public bool has_gold_subscription { get; set; }
        public int num_friends { get; set; }
        public Features features { get; set; }
        public bool can_edit_name { get; set; }
        public bool verified { get; set; }
        public bool new_modmail_exists { get; set; }
        public bool pref_autoplay { get; set; }
        public int coins { get; set; }
        public bool has_paypal_subscription { get; set; }
        public bool has_subscribed_to_premium { get; set; }
        public string id { get; set; }
        public bool has_stripe_subscription { get; set; }
        public bool can_create_subreddit { get; set; }
        public bool over_18 { get; set; }
        public bool is_gold { get; set; }
        public bool is_mod { get; set; }
        public object suspension_expiration_utc { get; set; }
        public bool has_verified_email { get; set; }
        public bool is_suspended { get; set; }
        public bool pref_video_autoplay { get; set; }
        public bool in_chat { get; set; }
        public bool has_android_subscription { get; set; }
        public bool in_redesign_beta { get; set; }
        public string icon_img { get; set; }
        public bool has_mod_mail { get; set; }
        public bool pref_nightmode { get; set; }
        public bool hide_from_robots { get; set; }
        public bool password_set { get; set; }
        public string modhash { get; set; }
        public int link_karma { get; set; }
        public bool accept_chats { get; set; }
        public bool force_password_reset { get; set; }
        public int inbox_count { get; set; }
        public bool pref_top_karma_subreddits { get; set; }
        public bool has_mail { get; set; }
        public bool pref_show_snoovatar { get; set; }
        public string name { get; set; }
        public int pref_clickgadget { get; set; }
        public double created { get; set; }
        public int gold_creddits { get; set; }
        public double created_utc { get; set; }
        public bool has_ios_subscription { get; set; }
        public bool pref_show_twitter { get; set; }
        public bool in_beta { get; set; }
        public int comment_karma { get; set; }
        public bool has_subscribed { get; set; }
        public bool accept_pms { get; set; }
    }

    public class About
    {
        public string kind { get; set; }
        public Data data { get; set; }
    }

}
