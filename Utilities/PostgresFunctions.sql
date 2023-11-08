-- Execute them on Supabase Database

-- compare_tags
CREATE OR REPLACE FUNCTION compare_tags(tag_name text)
RETURNS integer AS
$$
DECLARE
    tag_id integer;
BEGIN
    tag_name := LOWER(tag_name);

    SELECT id INTO tag_id FROM tags WHERE LOWER(name) = tag_name;

    RETURN tag_id;
END;
$$
LANGUAGE plpgsql;

-- get_multimedia_by_post_key
CREATE OR REPLACE FUNCTION get_multimedia_by_post_key(post_key uuid)
RETURNS TABLE (
    id int8,
    created_at timestamptz,
    key uuid,
    post_id int8,
    bucket_url character varying,
    file_name character varying,
    file_extension character varying,
    file_size_kb double precision
)
AS $$
BEGIN
    RETURN QUERY
    SELECT mul.*
    FROM multimedia mul
    LEFT JOIN posts post ON post.id = mul.post_id
    WHERE post.key = post_key;
END;
$$ LANGUAGE plpgsql;

-- get_tags_for_post
CREATE OR REPLACE FUNCTION get_tags_for_post(post_key uuid)
RETURNS TABLE (tag_key uuid, tag_name varchar) AS $$
BEGIN
    RETURN QUERY
    SELECT t.key, t.name
    FROM tagspost tp
    JOIN tags t ON tp.tag_id = t.id
    JOIN posts p ON tp.post_id = p.id
    WHERE p.key = get_tags_for_post.post_key;
END;
$$ LANGUAGE plpgsql;

-- insert_into_multimedia
CREATE OR REPLACE FUNCTION insert_into_multimedia(
  post_id int8,
  bucket_url text,
  file_name text,
  file_extension text,
  file_size_kb double precision
)
RETURNS integer AS $$
DECLARE
  inserted_id integer;
BEGIN
  INSERT INTO multimedia("key", "created_at", "post_id", "bucket_url", "file_name", "file_extension", "file_size_kb")
  VALUES (uuid_generate_v4(), now(), post_id, bucket_url, file_name, file_extension, file_size_kb)
  RETURNING id INTO inserted_id;

  RETURN inserted_id;
EXCEPTION
  WHEN others THEN
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- insert_into_posts
CREATE OR REPLACE FUNCTION insert_into_posts(
  title text,
  content text,
  userId int8,
  active boolean
)
RETURNS integer AS $$
DECLARE
  inserted_id integer;
BEGIN
  INSERT INTO posts("key", "created_at", "title", "content", "active", "user_id")
  VALUES (uuid_generate_v4(), now(), title, content, active, userId)
  RETURNING id INTO inserted_id;

  RETURN inserted_id;
EXCEPTION
  WHEN others THEN
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- insert_into_tag
CREATE OR REPLACE FUNCTION insert_into_tag(tag_name text)
RETURNS integer AS
$$
DECLARE
    inserted_id integer;
BEGIN
    BEGIN
        INSERT INTO tags (key, created_at, name)
        VALUES (uuid_generate_v4(), now(), tag_name)
        RETURNING id INTO inserted_id;

        RETURN inserted_id;
    EXCEPTION
        WHEN OTHERS THEN
            RETURN NULL;
    END;
END;
$$
LANGUAGE plpgsql;

-- insert_into_tagspost
CREATE OR REPLACE FUNCTION insert_into_tagspost(
    post_id int8,
    tag_id int8
)
RETURNS integer AS
$$
DECLARE
    inserted_id integer;
BEGIN
    BEGIN
        INSERT INTO tagspost (key, created_at, post_id, tag_id)
        VALUES (uuid_generate_v4(), now(), post_id, tag_id)
        RETURNING id INTO inserted_id;

        RETURN inserted_id;
    EXCEPTION
        WHEN OTHERS THEN
            RETURN NULL;
    END;
END;
$$
LANGUAGE plpgsql;
