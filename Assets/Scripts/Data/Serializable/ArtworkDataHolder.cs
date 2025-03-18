using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ArtworkDataHolder
{
    public string title;
    public string description;
    public List<ArtistDataHolder> artists;
    public int year;
    public string location;
    public List<string> artwork_image_references;
    public double latitude;
    public double longitude;
    public float max_distance;
    public bool place_right;
    public string creation_time; // stored as ISO string
    public string update_time;   // stored as ISO string
    // Removed media_content string property
    public List<MediaContentDataHolder> media_content_list;
    public string artwork_id;
    public string preset;
    public string alt_scene;
    private List<string> cache = new List<string>();

    public static ArtworkDataHolder ToHolder(ArtworkData artwork)
    {
        ArtworkDataHolder holder = new ArtworkDataHolder();
        holder.title = artwork.title;
        holder.description = artwork.description;
        holder.artists = new List<ArtistDataHolder>();
        if (artwork.artists != null)
        {
            foreach (var artist in artwork.artists)
            {
                holder.artists.Add(ArtistDataHolder.FromArtistData(artist));
            }
        }
        holder.year = artwork.year;
        holder.location = artwork.location;
        holder.artwork_image_references = new List<string>(artwork.artwork_image_references);
        holder.latitude = artwork.latitude;
        holder.longitude = artwork.longitude;
        holder.max_distance = artwork.max_distance;
        holder.place_right = artwork.place_right;
        holder.creation_time = artwork.creation_date_time.ToString("o");
        holder.update_time = artwork.update_date_time.ToString("o");
        holder.cache = new List<string>(artwork.cached);
        holder.preset = artwork.preset;
        holder.alt_scene = artwork.alt_scene;

        // Convert the list of MediaContentData to holder objects
        holder.media_content_list = new List<MediaContentDataHolder>();
        if (artwork.content_list != null)
        {
            foreach (var media in artwork.content_list)
            {
                holder.media_content_list.Add(MediaContentDataHolder.FromTransformsData(media));
            }
        }
        holder.artwork_id = artwork.id;
        return holder;
    }

    public static ArtworkData FromHolder(ArtworkDataHolder holder)
    {
        ArtworkData artwork = new ArtworkData();
        artwork.title = holder.title;
        artwork.description = holder.description;
        artwork.artists = new List<ArtistData>();
        if (holder.artists != null)
        {
            foreach (var artistHolder in holder.artists)
            {
                artwork.artists.Add(ArtistDataHolder.ToArtistData(artistHolder));
            }
        }
        artwork.year = holder.year;
        artwork.location = holder.location;
        artwork.artwork_image_references = new List<string>(holder.artwork_image_references);
        artwork.latitude = holder.latitude;
        artwork.longitude = holder.longitude;
        artwork.max_distance = holder.max_distance;
        artwork.place_right = holder.place_right;
        artwork.creation_date_time = DateTime.Parse(holder.creation_time);
        artwork.update_date_time = DateTime.Parse(holder.update_time);
        artwork.cached = new List<string>(holder.cache);
        artwork.preset = holder.preset;
        artwork.alt_scene = holder.alt_scene;

        // Convert back the list of media content holders to MediaContentData objects
        artwork.content_list = new List<MediaContentData>();
        if (holder.media_content_list != null)
        {
            foreach (var mediaHolder in holder.media_content_list)
            {
                artwork.content_list.Add(MediaContentDataHolder.ToTransformsData(mediaHolder));
            }
        }
        artwork.id = holder.artwork_id;
        return artwork;
    }
}

[Serializable]
public class MediaContentDataHolder
{
    // Now holds the media content string from MediaContentData
    public string media_content;
    public PositionOffsetHolder position_offset;
    public RotationHolder rotation;
    public ScaleHolder scale;

    public static MediaContentDataHolder FromTransformsData(MediaContentData mediaContent)
    {
        if (mediaContent == null) return null;
        MediaContentDataHolder holder = new MediaContentDataHolder();
        holder.media_content = mediaContent.media_content;
        holder.position_offset = PositionOffsetHolder.FromPositionOffset(mediaContent.transforms.position_offset);
        holder.rotation = RotationHolder.FromRotation(mediaContent.transforms.rotation);
        holder.scale = ScaleHolder.FromScale(mediaContent.transforms.scale);
        return holder;
    }

    public static MediaContentData ToTransformsData(MediaContentDataHolder holder)
    {
        if (holder == null) return null;
        MediaContentData mediaContent = new MediaContentData();
        mediaContent.media_content = holder.media_content;
        mediaContent.transforms.position_offset = PositionOffsetHolder.ToPositionOffset(holder.position_offset);
        mediaContent.transforms.rotation = RotationHolder.ToRotation(holder.rotation);
        mediaContent.transforms.scale = ScaleHolder.ToScale(holder.scale);
        return mediaContent;
    }
}

[Serializable]
public class PositionOffsetHolder
{
    public float x_offset;
    public float y_offset;
    public float z_offset;

    public static PositionOffsetHolder FromPositionOffset(PositionOffset offset)
    {
        if (offset == null) return null;
        return new PositionOffsetHolder 
        { 
            x_offset = offset.x_offset, 
            y_offset = offset.y_offset, 
            z_offset = offset.z_offset 
        };
    }

    public static PositionOffset ToPositionOffset(PositionOffsetHolder holder)
    {
        if (holder == null) return null;
        return new PositionOffset 
        { 
            x_offset = holder.x_offset, 
            y_offset = holder.y_offset, 
            z_offset = holder.z_offset 
        };
    }
}

[Serializable]
public class ScaleHolder
{
    public float x_scale;
    public float y_scale;
    public float z_scale;

    public static ScaleHolder FromScale(Scale scale)
    {
        if (scale == null) return null;
        return new ScaleHolder 
        { 
            x_scale = scale.x_scale, 
            y_scale = scale.y_scale, 
            z_scale = scale.z_scale 
        };
    }

    public static Scale ToScale(ScaleHolder holder)
    {
        if (holder == null) return null;
        return new Scale 
        { 
            x_scale = holder.x_scale, 
            y_scale = holder.y_scale, 
            z_scale = holder.z_scale 
        };
    }
}

[Serializable]
public class RotationHolder
{
    public float x_rotation;
    public float y_rotation;
    public float z_rotation;

    public static RotationHolder FromRotation(Rotation rotation)
    {
        if (rotation == null) return null;
        return new RotationHolder() 
        { 
            x_rotation = rotation.x_rotation, 
            y_rotation = rotation.y_rotation, 
            z_rotation = rotation.z_rotation 
        };
    }

    public static Rotation ToRotation(RotationHolder holder)
    {
        if (holder == null) return null;
        return new Rotation 
        { 
            x_rotation = holder.x_rotation, 
            y_rotation = holder.y_rotation, 
            z_rotation = holder.z_rotation 
        };
    }
}
