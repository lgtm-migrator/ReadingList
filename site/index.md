---
layout : layout
---

<section>
	<!--<ul class="posts">-->
		{% for post in site.posts  limit:5 %}
			<!--<li>-->
				<article>
					{% if forloop.first and post.layout == "post" %}
						<h1><a href="{{ post.url }}">{{ post.title }}</a></h1>
					{% else %}
						<h2><a class="postlink" href="{{ post.url }}">{{ post.title }}</a></h2>
					{% endif %}
					<time class="postdate" pubdate="{{ post.date | date: "%Y-%m-%d" }}">{{ post.date | date: "%e %B %Y" }}</time>
						<ul class="book-infos">
							<li>{{ post.author }}</li>
							<li>{{ post.editor }}</li>
							<li>{{ post.isbn }}</li>
						</ul>
					{{ post.content }}
				</article>
			<!--</li>-->
		{% endfor %}
	<!--</ul>-->
</section>

<section class="archive">
	{% if site.posts.size > 5 %}
	<h3>Anciens</h3>
	<ul>
		{% for post in site.posts offset:5 %}
		<li>
			<time class="olderpostdate" datetime="{{ post.date | date: "%Y-%m-%d" }}"> {{ post.date | date: "%d %b"  }} </time> <a class="postlink" href="{{ post.url }}">{{ post.title }}</a>
		</li>
		{% endfor %}
	</ul>
	{% endif %}
<section>